using UnityEngine;
using UnityEngine.InputSystem;

public class MotorController : MonoBehaviour
{
    [Header("Configuración Motor")]
    public bool motorEncendido = false;
    public float rpm = 0f;
    public float minRPM = 800f;
    public float maxRPM = 7000f;
    public float rpmIncreaseRate = 2000f;
    public float rpmDecreaseRate = 1500f;

    [Header("Configuración Transmisión")]
    public float velocidadActual = 0f;
    public float relacionDiferencial = 3.42f;
    public float radioRueda = 0.33f; // En metros
    public float rpmMinimaCambioArriba = 2500f;
    public float rpmMaximaCambioAbajo = 1000f;
    public int marchaActual = 0;
    public float[] relacionesMarcha = { 0f, 3.82f, 2.20f, 1.52f, 1.22f, 1.02f, 0.84f }; // Neutral y 6 marchas

    private Keyboard teclado;

    void Awake()
    {
        teclado = Keyboard.current;
    }

    void Update()
    {
        if (motorEncendido)
        {
            // Aceleración
            if (teclado.wKey.isPressed && marchaActual > 0)
            {
                rpm += rpmIncreaseRate * Time.deltaTime;
                rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
            }
            // Desaceleración en marcha
            else if (marchaActual > 0)
            {
                rpm -= rpmDecreaseRate * Time.deltaTime;
                rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
            }
            // Ralentí en neutral
            else if (marchaActual == 0)
            {
                // En neutral, las RPM bajan más lento y se mantienen en ralentí
                if (teclado.wKey.isPressed)
                {
                    rpm += (rpmIncreaseRate * 0.5f) * Time.deltaTime;
                    rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
                }
                else
                {
                    rpm -= (rpmDecreaseRate * 0.2f) * Time.deltaTime;
                    rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
                }
            }
        }
        else
        {
            rpm = 0f;
            velocidadActual = 0f;
        }

        // Encender/apagar motor
        if (teclado.eKey.wasPressedThisFrame)
        {
            motorEncendido = true;
            if (rpm < minRPM) rpm = minRPM;
        }

        if (teclado.qKey.wasPressedThisFrame)
        {
            motorEncendido = false;
        }

        // Cambio de marcha hacia arriba
        if (teclado.upArrowKey.wasPressedThisFrame)
        {
            if (marchaActual < relacionesMarcha.Length - 1)
            {
                // Solo permitir cambio si las RPM son suficientes
                if (marchaActual == 0 || rpm >= rpmMinimaCambioArriba)
                {
                    marchaActual++;
                    Debug.Log("Marcha actual: " + marchaActual);
                    BajarRPMCambioMarcha();
                }
                else
                {
                    Debug.Log("RPM insuficientes para cambiar a marcha superior");
                }
            }
        }

        // Cambio de marcha hacia abajo
        if (teclado.downArrowKey.wasPressedThisFrame)
        {
            if (marchaActual > 0)
            {
                // Permitir bajar a neutral desde primera o a marcha inferior si no excede RPM
                if (marchaActual == 1 || rpm <= rpmMaximaCambioAbajo || CalcularRPMTrasCambioAbajo() <= maxRPM)
                {
                    marchaActual--;
                    Debug.Log("Marcha actual: " + marchaActual);
                    
                    // Si cambiamos a una marcha inferior (no a neutral), ajustamos RPM hacia arriba
                    if (marchaActual > 0)
                    {
                        AjustarRPMCambioAbajo();
                    }
                }
                else
                {
                    Debug.Log("RPM demasiado altas para cambiar a marcha inferior");
                }
            }
        }

        // Calcular velocidad basada en RPM y relación de marcha
        CalcularVelocidad();
    }

    private void BajarRPMCambioMarcha()
    {
        rpm *= 0.6f; // Al hacer cambio reducimos rpm al 60%
        rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
    }

    private void AjustarRPMCambioAbajo()
    {
        rpm *= 1.4f; // Al bajar marcha, RPM aumentan aproximadamente un 40%
        rpm = Mathf.Clamp(rpm, minRPM, maxRPM);
    }

    private float CalcularRPMTrasCambioAbajo()
    {
        // Calcula las RPM potenciales si bajamos una marcha
        if (marchaActual <= 1) return 0; // No aplicable para primera o neutral
        
        float relacionActual = relacionesMarcha[marchaActual];
        float relacionInferior = relacionesMarcha[marchaActual - 1];
        
        return rpm * (relacionInferior / relacionActual);
    }

    private void CalcularVelocidad()
    {
        if (marchaActual > 0 && motorEncendido)
        {
            // Fórmula: v = (rpm * pi * diámetro de rueda) / (60 * relación de marcha * relación diferencial)
            velocidadActual = (rpm * Mathf.PI * (2 * radioRueda)) / 
                              (60 * relacionesMarcha[marchaActual] * relacionDiferencial);
                              
            // Convertir a km/h
            velocidadActual *= 3.6f;
        }
        else
        {
            velocidadActual = 0f;
        }
        
        // Mostrar velocidad en consola (opcional, puedes comentar o quitar esta línea)
        if (Time.frameCount % 30 == 0) // Actualizar cada 30 frames para no saturar la consola
        {
            Debug.Log("Velocidad: " + velocidadActual.ToString("F1") + " km/h");
        }
    }
}
