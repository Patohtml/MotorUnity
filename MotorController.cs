using UnityEngine;
using UnityEngine.InputSystem;

public class MotorController : Simulator
{
    [Header("Información del Motor")]
    public bool motorEncendido = false;
    public float rpm = 0f;
    public int marchaActual = 0;
    public float velocidadActual = 0f;
    [SerializeField] private Creadores creador = Creadores.Fratti_Lucas;
    
    // Variables ocultas del inspector
    private float minRPM = 800f;
    private float maxRPM = 7000f;
    private float rpmIncreaseRate = 2000f;
    private float rpmDecreaseRate = 1500f;
    private float rpmDecreaseRateMotorApagado = 800f;
    private float relacionDiferencial = 3.42f;
    private float radioRueda = 0.33f;
    private float rpmMinimaCambioArriba = 2500f;
    private float rpmMaximaCambioAbajo = 1000f;
    private float[] relacionesMarcha = { 0f, 3.82f, 2.20f, 1.52f, 1.22f, 1.02f, 0.84f };

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
            // Si el motor está apagado, las RPM disminuyen gradualmente
            rpm -= rpmDecreaseRateMotorApagado * Time.deltaTime;
            rpm = Mathf.Max(0f, rpm); // No permitir RPM negativas
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
        if (marchaActual > 0 && rpm > 0)
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

    public void Start()
    {
        AsignarCreador(Creadores.Fratti_Lucas); 
    }

    public override void Describir()
    {
        Debug.Log("MotorController es un controlador de motor de un automóvil.");
    }

    public override void AsignarCreador(Creadores creador)
    {
        Debug.Log("Este componente fue creado por: " + creador.ToString());
    }
}   

