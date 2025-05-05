using UnityEngine;


public abstract class Simulator : MonoBehaviour
{
    private Creadores creadorSimulator;

    /// <summary>
    /// Esta propiedad devuelve la lista de creadores posibles del juego
    /// </summary>
    public Creadores CreadoresSimulator
    {
        get { return creadorSimulator; }
        set { creadorSimulator = value; }
    }

    /// <summary>
    /// Permite describir que hace el objeto heredado
    /// </summary>
    public virtual void Describir()
    {
        Debug.Log("Simulator es un simulador de manejo de automóviles.");
    }

    public abstract void AsignarCreador(Creadores creadores);
}