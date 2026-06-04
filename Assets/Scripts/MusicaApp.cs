using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la aplicación de música del celular.
/// Se activa cuando el estrés supera el 80% y reduce el estrés a la mitad
/// tras escuchar 10 segundos de música.
/// 
/// CÓMO USAR:
/// 1. Crear un objeto vacío en la escena y asignarle este script.
/// 2. Asignar todas las referencias en el Inspector de Unity.
/// 3. Agregar los AudioClip de las canciones.
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
    [Tooltip("Las canciones disponibles (agregar exactamente 2)")]
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

    // ── REFERENCIA AL SISTEMA DE ESTRÉS ──────────────────────────────────────
    [Header("Sistema de Estrés")]
    [Tooltip("Referencia al EmotionalStateManager de la escena")]
    [SerializeField] private EmotionalStateManager emotionalStateManager;

    // ── VARIABLES INTERNAS (no tocar en el Inspector) ─────────────────────────
    private int cancionActualIndex = 0;
    private bool appDesbloqueada = false;
    private bool escuchando = false;
    private float tiempoEscuchado = 0f;
    private const float TIEMPO_REQUERIDO = 10f; // segundos que debe escuchar

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Arrancar con la app bloqueada
        BloquearApp();

        // Asegurarse de que el pop-up esté oculto al inicio
        if (popUpEstres != null) popUpEstres.SetActive(false);

        // Asegurarse de que la pantalla de música esté oculta al inicio
        if (pantallaMusica != null) pantallaMusica.SetActive(false);

        // Conectar botones
        if (botonAceptarPopUp != null)
            botonAceptarPopUp.onClick.AddListener(AlAceptarPopUp);

        if (botonPlayPausa != null)
            botonPlayPausa.onClick.AddListener(TogglePlayPausa);

        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(SiguienteCancion);

        // Cargar la primera canción sin reproducirla
        CargarCancion(0);
    }

    private void Update()
    {
        // Si el jugador está escuchando música, acumular tiempo
        if (escuchando && audioSource != null && audioSource.isPlaying)
        {
            tiempoEscuchado += Time.deltaTime;

            if (tiempoEscuchado >= TIEMPO_REQUERIDO)
            {
                ReducirEstres();
            }
        }
    }

    // ── MÉTODOS PÚBLICOS ──────────────────────────────────────────────────────

    /// <summary>
    /// Llamar desde EmotionalStateManager cuando el estrés supere el 80%.
    /// Muestra el pop-up de alerta.
    /// </summary>
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
        if (iconoMusica != null) iconoMusica.color = colorBloqueado;
        if (botonIconoMusica != null) botonIconoMusica.interactable = false;
    }

    private void DesbloquearApp()
    {
        appDesbloqueada = true;
        if (iconoMusica != null) iconoMusica.color = colorDesbloqueado;
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

    private void SiguienteCancion()
    {
        if (canciones.Length == 0) return;

        // Reiniciar el tiempo de escucha al cambiar de canción
        tiempoEscuchado = 0f;

        cancionActualIndex = (cancionActualIndex + 1) % canciones.Length;
        CargarCancion(cancionActualIndex);

        // Si estaba reproduciendo, continuar con la nueva canción
        if (escuchando)
        {
            audioSource.Play();
        }
    }

    private void CargarCancion(int index)
    {
        if (canciones.Length == 0 || index >= canciones.Length) return;

        if (audioSource != null)
        {
            audioSource.clip = canciones[index];
        }

        if (textoNombreCancion != null && canciones[index] != null)
        {
            textoNombreCancion.text = canciones[index].name;
        }
    }

    private void MostrarIconoPlay(bool mostrarPlay)
    {
        if (iconoPlay != null) iconoPlay.SetActive(mostrarPlay);
        if (iconoPausa != null) iconoPausa.SetActive(!mostrarPlay);
    }

    private void ReducirEstres()
    {
        // Dejar de contar para que no se llame múltiples veces
        escuchando = false;
        tiempoEscuchado = 0f;

        // Parar la música
        if (audioSource != null) audioSource.Stop();
        MostrarIconoPlay(true);

        // Reducir el estrés a la mitad usando el sistema existente del juego
        if (emotionalStateManager != null)
        {
            int estresActual = emotionalStateManager.stress;
            int reduccion = -(estresActual / 2); // número negativo para restar
            emotionalStateManager.ModifyState(reduccion, 0, 0);
            Debug.Log($"[MusicaApp] Estrés reducido. De {estresActual} a {emotionalStateManager.stress}");
        }
    }
}
