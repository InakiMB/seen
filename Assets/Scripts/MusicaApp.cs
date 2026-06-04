using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la aplicación de música del celular.
/// Se activa cuando el estrés supera el 80% y reduce el estrés a la mitad
/// tras escuchar 10 segundos de música.
///
/// NOVEDADES v2:
///  - Portada de álbum cambia según la canción activa.
///  - Slider de progreso de canción (solo lectura, el usuario no lo arrastra).
///  - La canción sigue reproduciéndose indefinidamente hasta que el usuario
///    presione Stop; ya NO se corta automáticamente a los 10 segundos.
///  - La reducción de estrés sigue ocurriendo al llegar a 10 s de escucha,
///    pero la música continúa sonando.
///  - Botón Stop para detener manualmente.
/// </summary>
public class MusicaApp : MonoBehaviour
{
    // ── REFERENCIA A LA APP ──────────────────────────────────────────────────
    [Header("Pantalla de la App")]
    [Tooltip("El panel completo de la app de música")]
    [SerializeField] private GameObject pantallaMusica;

    // ── ICONO DEL CELULAR ─────────────────────────────────────────────────────
    [Header("Icono de Música en el Celular")]
    [Tooltip("El Image del ícono de música en la pantalla de inicio del celu")]
    [SerializeField] private Image iconoMusica;
    [Tooltip("Botón del ícono de música (para activar/desactivar el click)")]
    [SerializeField] private Button botonIconoMusica;
    [Tooltip("Color cuando la app está BLOQUEADA")]
    [SerializeField] private Color colorBloqueado = Color.gray;
    [Tooltip("Color cuando la app está DISPONIBLE")]
    [SerializeField] private Color colorDesbloqueado = Color.green;

    // ── POP-UP DE ESTRÉS ──────────────────────────────────────────────────────
    [Header("Pop-up de Estrés Alto")]
    [Tooltip("El panel del pop-up que avisa sobre el estrés alto")]
    [SerializeField] private GameObject popUpEstres;
    [Tooltip("Botón 'OK' o 'Aceptar' dentro del pop-up")]
    [SerializeField] private Button botonAceptarPopUp;

    // ── REPRODUCTOR DE MÚSICA ─────────────────────────────────────────────────
    [Header("Reproductor")]
    [Tooltip("El AudioSource que reproducirá las canciones")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Las canciones disponibles (agregar exactamente 2 o más)")]
    [SerializeField] private AudioClip[] canciones;
    [Tooltip("Texto que muestra el nombre de la canción actual")]
    [SerializeField] private TMP_Text textoNombreCancion;
    [Tooltip("Botón de Play/Pausa")]
    [SerializeField] private Button botonPlayPausa;
    [Tooltip("Ícono de Play (mostrar cuando está pausado)")]
    [SerializeField] private GameObject iconoPlay;
    [Tooltip("Ícono de Pausa (mostrar cuando está reproduciendo)")]
    [SerializeField] private GameObject iconoPausa;
    [Tooltip("Botón para pasar a la siguiente canción")]
    [SerializeField] private Button botonSiguiente;
    [Tooltip("Botón Stop: detiene la reproducción completamente")]
    [SerializeField] private Button botonStop;

    // ── PORTADA DE ÁLBUM ──────────────────────────────────────────────────────
    [Header("Portada de Álbum")]
    [Tooltip("El componente Image en blanco donde se mostrará la portada")]
    [SerializeField] private Image imagenPortada;
    [Tooltip("Sprites de portada para cada canción (mismo orden que 'canciones')")]
    [SerializeField] private Sprite[] portadas;

    // ── SLIDER DE PROGRESO ────────────────────────────────────────────────────
    [Header("Barra de Progreso")]
    [Tooltip("El Slider que indica en qué punto de la canción va")]
    [SerializeField] private Slider sliderProgreso;
    [Tooltip("(Opcional) Texto que muestra el tiempo actual — ej: 1:23 / 3:45")]
    [SerializeField] private TMP_Text textoTiempo;

    // ── REFERENCIA AL SISTEMA DE ESTRÉS ──────────────────────────────────────
    [Header("Sistema de Estrés")]
    [Tooltip("Referencia al EmotionalStateManager de la escena")]
    [SerializeField] private EmotionalStateManager emotionalStateManager;

    // ── VARIABLES INTERNAS ────────────────────────────────────────────────────
    private int cancionActualIndex = 0;
    private bool appDesbloqueada = false;
    private bool escuchando = false;
    private float tiempoEscuchado = 0f;
    private bool estresYaReducido = false;           // <-- NUEVO: evita reducir 2 veces por canción
    private const float TIEMPO_REQUERIDO = 10f;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        BloquearApp();

        if (popUpEstres != null)     popUpEstres.SetActive(false);
        if (pantallaMusica != null)  pantallaMusica.SetActive(false);

        // Slider: sólo lectura (el jugador no puede arrastrarlo)
        if (sliderProgreso != null)
        {
            sliderProgreso.interactable = false;
            sliderProgreso.minValue = 0f;
            sliderProgreso.maxValue = 1f;
            sliderProgreso.value    = 0f;
        }

        // Conectar botones
        if (botonAceptarPopUp != null)
            botonAceptarPopUp.onClick.AddListener(AlAceptarPopUp);

        if (botonPlayPausa != null)
            botonPlayPausa.onClick.AddListener(TogglePlayPausa);

        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(SiguienteCancion);

        if (botonStop != null)
            botonStop.onClick.AddListener(DetenerMusica);

        CargarCancion(0);
    }

    private void Update()
    {
        if (escuchando && audioSource != null && audioSource.isPlaying)
        {
            tiempoEscuchado += Time.deltaTime;

            // Reducir estrés UNA SOLA VEZ al llegar a 10 s — música sigue sonando
            if (!estresYaReducido && tiempoEscuchado >= TIEMPO_REQUERIDO)
            {
                estresYaReducido = true;
                ReducirEstres();
            }
        }

        // Actualizar slider y texto de tiempo cada frame
        ActualizarProgreso();
    }

    // ── MÉTODOS PÚBLICOS ──────────────────────────────────────────────────────

    public void MostrarPopUpEstres()
    {
        if (popUpEstres != null)
            popUpEstres.SetActive(true);
    }

    // ── MÉTODOS PRIVADOS ──────────────────────────────────────────────────────

    private void AlAceptarPopUp()
    {
        if (popUpEstres != null) popUpEstres.SetActive(false);
        DesbloquearApp();
    }

    private void BloquearApp()
    {
        appDesbloqueada = false;
        if (iconoMusica     != null) iconoMusica.color              = colorBloqueado;
        if (botonIconoMusica != null) botonIconoMusica.interactable = false;
    }

    private void DesbloquearApp()
    {
        appDesbloqueada = true;
        if (iconoMusica      != null) iconoMusica.color             = colorDesbloqueado;
        if (botonIconoMusica != null) botonIconoMusica.interactable = true;
        Debug.Log("[MusicaApp] App de música desbloqueada.");
    }

    private void TogglePlayPausa()
    {
        if (audioSource == null || canciones.Length == 0) return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            escuchando = false;
            MostrarIconoPlay(true);
        }
        else
        {
            audioSource.Play();
            escuchando = true;
            MostrarIconoPlay(false);
        }
    }

    // Detiene completamente la reproducción (botón Stop)
    private void DetenerMusica()
    {
        if (audioSource != null) audioSource.Stop();
        escuchando = false;
        MostrarIconoPlay(true);

        // Resetear slider
        if (sliderProgreso != null) sliderProgreso.value = 0f;
        if (textoTiempo    != null) textoTiempo.text     = FormatearTiempo(0f) + " / " + FormatearTiempo(ObtenerDuracion());
    }

    private void SiguienteCancion()
    {
        if (canciones.Length == 0) return;

        tiempoEscuchado  = 0f;
        estresYaReducido = false;   // resetear para la nueva canción

        cancionActualIndex = (cancionActualIndex + 1) % canciones.Length;
        CargarCancion(cancionActualIndex);

        if (escuchando)
            audioSource.Play();
    }

    private void CargarCancion(int index)
    {
        if (canciones.Length == 0 || index >= canciones.Length) return;

        if (audioSource != null)
            audioSource.clip = canciones[index];

        // Nombre de la canción
        if (textoNombreCancion != null && canciones[index] != null)
            textoNombreCancion.text = canciones[index].name;

        // Portada
        if (imagenPortada != null)
        {
            if (portadas != null && index < portadas.Length && portadas[index] != null)
                imagenPortada.sprite = portadas[index];
            else
                imagenPortada.sprite = null;   // deja el blanco si no hay portada
        }

        // Resetear slider al inicio
        if (sliderProgreso != null) sliderProgreso.value = 0f;
    }

    private void MostrarIconoPlay(bool mostrarPlay)
    {
        if (iconoPlay  != null) iconoPlay.SetActive(mostrarPlay);
        if (iconoPausa != null) iconoPausa.SetActive(!mostrarPlay);
    }

    // Actualiza el slider y el texto de tiempo cada frame
    private void ActualizarProgreso()
    {
        if (audioSource == null || audioSource.clip == null) return;

        float duracion = audioSource.clip.length;
        if (duracion <= 0f) return;

        float progreso = audioSource.time / duracion;

        if (sliderProgreso != null)
            sliderProgreso.value = progreso;

        if (textoTiempo != null)
            textoTiempo.text = FormatearTiempo(audioSource.time) + " / " + FormatearTiempo(duracion);
    }

    private float ObtenerDuracion()
    {
        if (audioSource != null && audioSource.clip != null)
            return audioSource.clip.length;
        return 0f;
    }

    // Convierte segundos a "m:ss"
    private string FormatearTiempo(float segundos)
    {
        int m = Mathf.FloorToInt(segundos / 60f);
        int s = Mathf.FloorToInt(segundos % 60f);
        return $"{m}:{s:00}";
    }

    // Reduce el estrés a la mitad SIN detener la música
    private void ReducirEstres()
    {
        if (emotionalStateManager != null)
        {
            int estresActual = emotionalStateManager.stress;
            int reduccion    = -(estresActual / 2);
            emotionalStateManager.ModifyState(reduccion, 0, 0);
            Debug.Log($"[MusicaApp] Estrés reducido. De {estresActual} a {emotionalStateManager.stress}. La música sigue sonando.");
        }
    }
}
