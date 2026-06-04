using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelularInterface : MonoBehaviour
{
    [SerializeField] private GameObject pantallaInicio;
    [SerializeField] private GameObject pantallaAppMsg;
    [SerializeField] private GameObject InterfazAppMsg;
    [SerializeField] private GameObject pantallaDiary;


    [SerializeField] private GameObject appActual;
    
    private Stack<GameObject> historialPantallasApp = new Stack<GameObject>();
    private GameObject pantallaActual;
    public GameObject AppActual { get => appActual;}
    public Stack<GameObject> HistorialPantallasApp { get => historialPantallasApp; set => historialPantallasApp = value; }
    public static CelularInterface instancia;
    private void Awake()
    {
        CelularManagent.celularInterface = this;
    }
    public void RetrocederPantalla()
    {
        if (HistorialPantallasApp.Count > 1)
        {
            pantallaActual = HistorialPantallasApp.Pop();
            pantallaActual.SetActive(false);
            pantallaActual = HistorialPantallasApp.Peek();
            pantallaActual.SetActive(true);

            Debug.Log($"Retrocedió a: {pantallaActual.name}. Elementos restantes en historial: {HistorialPantallasApp.Count}");
        }
        else
        {
            Debug.Log("No hay más historial. Ya estás en el menú de inicio.");
            VolverInicio();
        }
    }
    public void VolverInicio()
    {
        LimpiarHistorial();
        CambiarApp(pantallaInicio);
    }
    public void AbrirAppMsg()
    {
        CambiarApp(pantallaAppMsg);
        AppMsgInterface managerMsg = InterfazAppMsg.GetComponent<AppMsgInterface>();
        managerMsg.AbrirInicio();
    }
    public void CambiarPantalla(GameObject pantallaActual)
    {
        HistorialPantallasApp.Push(pantallaActual);
        Debug.Log($"Avanzó a: {pantallaActual.name}. Guardada en historial: {HistorialPantallasApp.Peek().name}");
    }
   public void CambiarApp(GameObject appAbierto)
{
    if (appActual != null && appActual != appAbierto)
        appActual.SetActive(false);

    appActual = appAbierto;
    appActual.SetActive(true);
}

    public void LimpiarHistorial()
    {
        HistorialPantallasApp.Clear();
        Debug.Log("Historial limpiado por completo.");
    }

    public void AbrirDiary()
    {
        CambiarApp(pantallaDiary);
    }
}
