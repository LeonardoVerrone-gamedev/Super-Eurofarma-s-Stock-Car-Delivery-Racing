using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController2D : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;

    [Header("Movement")]
    public float acceleration = 10f;
    public float maxSpeed = 20f;
    public float reverseSpeed = 10f;
    public float brakeForce = 20f;
    public float minSteerSpeed = 0.5f;

    [Header("Steering")]
    public float steerStrength = 200f;
    public float steerAtMaxSpeed = 0.5f;

    [Header("Low Speed Steering")]
    public float lowSpeedThreshold = 0.2f; // 20% da velocidade máxima
    public AnimationCurve lowSpeedSteerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Drift")]
    public float grip = 0.9f;
    public float driftGrip = 0.3f;
    public float driftSteerBoost = 1.5f;

    [Header("Drift Boost")]
    public float driftBoostForce = 8f;
    public float driftBoostDuration = 0.3f;
    public AnimationCurve driftBoostCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    float driftTimer;
    bool wasDrifting;
    float boostTimer;

    [Header("Gears (Arcade Feel)")]
    public int totalGears = 5;
    public float gearSmooth = 5f; // suaviza troca
    public AnimationCurve gearPowerCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.4f);


    // Inputs
    Vector2 steerInput;
    float accelInput;
    float brakeInput;
    bool isDrifting;

    int currentGear = 1;
    float gearFactor; // 0–1 dentro da marcha

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        UpdateGear();
        ApplyEngine();
        ApplySteering();
        ApplyFriction();
        ApplyDriftBoost();
    }

    void UpdateGear()
    {
        float speedPercent = rb.linearVelocity.magnitude / maxSpeed;
        float gearFloat = speedPercent * totalGears;

        int targetGear = Mathf.Clamp(Mathf.FloorToInt(gearFloat) + 1, 1, totalGears);

        currentGear = targetGear;

        float targetGearFactor = gearFloat % 1f;
        gearFactor = Mathf.Lerp(gearFactor, targetGearFactor, Time.fixedDeltaTime * gearSmooth);
    }

    float GetGearMultiplier()
    {
        // Avalia curva (mais força em baixa, menos em alta)
        return gearPowerCurve.Evaluate((currentGear - 1 + gearFactor) / totalGears);
    }

    void ApplyEngine()
    {
        Vector2 forward = transform.up;
        float forwardSpeed = Vector2.Dot(rb.linearVelocity, forward);
        float gearMultiplier = GetGearMultiplier();

        bool isAlmostStopped = rb.linearVelocity.magnitude < 0.2f;

        // aceleração normal
        if (accelInput > 0)
        {
            if (forwardSpeed < maxSpeed)
            {
                rb.AddForce(forward * accelInput * acceleration * gearMultiplier, ForceMode2D.Force);
            }
        }

        float reverseVel = Vector2.Dot(rb.linearVelocity, forward); // forward = positivo, ré = negativo

        if (brakeInput > 0)
        {
            // só considera quando está indo pra trás ou parado
            if (reverseVel > -reverseSpeed)
            {
                // quanto mais perto do limite, menos força aplica
                float speedFactor = Mathf.InverseLerp(0, -reverseSpeed, reverseVel);
                float power = Mathf.Lerp(1f, 0f, speedFactor);

                rb.AddForce(-forward * brakeInput * acceleration * 0.75f * power, ForceMode2D.Force);
            }
        }
    }

    void ApplySteering()
    {
        float speed = rb.linearVelocity.magnitude;

        float speedPercent = speed / maxSpeed;

        // controle fino de steer em baixa velocidade
        float lowSpeedFactor = lowSpeedSteerCurve.Evaluate(speedPercent / lowSpeedThreshold);

        // clamp pra evitar passar de 1
        lowSpeedFactor = Mathf.Clamp01(lowSpeedFactor);

        float speedFactor = speedPercent;
        float steerMultiplier = Mathf.Lerp(1f, steerAtMaxSpeed, speedFactor);

        float driftMultiplier = isDrifting ? driftSteerBoost : 1f;

        float finalSteer = steerInput.x 
            * steerStrength 
            * steerMultiplier 
            * driftMultiplier 
            * lowSpeedFactor
            * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation - finalSteer);
    }

    void ApplyFriction()
    {
        Vector2 forward = transform.up;
        Vector2 right = transform.right;

        float targetGrip = isDrifting ? driftGrip : grip;
        float currentGrip = Mathf.Lerp(grip, targetGrip, isDrifting ? 1f : 0.2f);

        float forwardVel = Vector2.Dot(rb.linearVelocity, forward);
        float sidewaysVel = Vector2.Dot(rb.linearVelocity, right);

        Vector2 newVelocity = forward * forwardVel + right * sidewaysVel * currentGrip;

        rb.linearVelocity = newVelocity;
    }

    void ApplyDriftBoost()
    {
        if (isDrifting)
        {
            driftTimer += Time.fixedDeltaTime;
        }

        if (boostTimer > 0)
        {
            Vector2 forward = transform.up;

            float t = 1f - (boostTimer / driftBoostDuration);
            float force = driftBoostCurve.Evaluate(t);

            rb.AddForce(forward * force * driftBoostForce, ForceMode2D.Force);

            boostTimer -= Time.fixedDeltaTime;
        }
    }

    // =========================
    // INPUT SYSTEM CALLBACKS
    // =========================

    public void OnSteer(InputAction.CallbackContext context)
    {
        steerInput = context.ReadValue<Vector2>();
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        accelInput = context.ReadValue<float>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        brakeInput = context.ReadValue<float>();
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isDrifting = true;
            driftTimer = 0f;
        }
        else if (context.canceled)
        {
            isDrifting = false;

            // ativa boost baseado no tempo de drift
            if (driftTimer > 0.2f)
            {
                boostTimer = driftBoostDuration * Mathf.Clamp01(driftTimer);
            }
        }
    }
}