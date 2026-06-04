using UnityEngine;
using TMPro;

/// <summary>
/// VERSIÓN MODIFICADA de EmotionalStateManager.
/// Cambios respecto al original:
///   - Ahora tiene una referencia a MusicaApp
///   - Cuando el estrés llega al 80% o más, muestra el pop-up de música
///   - Solo muestra el pop-up UNA VEZ por "episodio" (no spam)
/// 
public class EmotionalStateManager : MonoBehaviour
{
    [SerializeField] private ReactiveUIManager reactiveUI;

    [SerializeField] private TMP_Text stressText;
    [SerializeField] private TMP_Text validationText;
    [SerializeField] private TMP_Text identityText;

    [Header("Estados emocionales")]
    [Range(0, 100)] public int stress = 20;
    [Range(0, 100)] public int validation = 50;
    [Range(0, 100)] public int identity = 50;

    // [NUEVO] Referencia a la app de música
    [Header("App de Música")]
    [Tooltip("Arrastrar aquí el objeto que tiene el script MusicaApp")]
    [SerializeField] private MusicaApp musicaApp;

    // [NUEVO] Para que el pop-up no se muestre repetidamente
    private bool popUpMostrado = false;
    private const int UMBRAL_ESTRES = 80;

    private void Start()
    {
        UpdateHUD();
        reactiveUI.SetStress(stress);
    }

    public void ModifyState(int stressChange, int validationChange, int identityChange)
    {
        stress = Mathf.Clamp(stress + stressChange, 0, 100);
        validation = Mathf.Clamp(validation + validationChange, 0, 100);
        identity = Mathf.Clamp(identity + identityChange, 0, 100);

        Debug.Log(
        $"Stress: {stress} | Validation: {validation} | Identity: {identity}");

        reactiveUI.SetStress(stress);

        // [NUEVO] Verificar si el estrés superó el umbral
        VerificarEstresAlto();

        UpdateHUD();
    }

    // [NUEVO] Método que revisa si hay que mostrar el pop-up
    private void VerificarEstresAlto()
    {
        if (stress >= UMBRAL_ESTRES && !popUpMostrado && musicaApp != null)
        {
            popUpMostrado = true;
            musicaApp.MostrarPopUpEstres();
            Debug.Log("[EmotionalStateManager] Estrés superó el 80%, mostrando pop-up de música.");
        }

        // Si el estrés baja del umbral, resetear para que pueda volver a aparecer
        // (por ejemplo, si usó la app de música y el estrés bajó)
        if (stress < UMBRAL_ESTRES)
        {
            popUpMostrado = false;
        }
    }

    private void UpdateHUD()
    {
        stressText.text = stress + "%";
        validationText.text = validation + "%";
        identityText.text = identity + "%";
    }
}

