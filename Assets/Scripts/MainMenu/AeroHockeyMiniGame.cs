using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AeroHockeyMiniGame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Elements")]
    public RectTransform arena;
    public RectTransform puck;
    public RectTransform playerPaddle;    // Игрок — СЛЕВА
    public RectTransform aiPaddle;        // ИИ — СПРАВА
    public TextMeshProUGUI scoreText;

    [Header("Game Settings")]
    public float paddleFollowSpeed = 15f;
    public float maxPuckSpeed = 12f;

    private Vector2 puckVelocity;
    private int playerScore = 0;
    private int aiScore = 0;
    private float lastHitTime = 0f;
    private bool isActive = false;

    private Vector2 targetPaddlePosition;
    private Vector2 previousPaddlePosition;
    private Vector2 previousAIPosition;
    private bool isDragging = false;

    private Vector2 ArenaHalfSize => arena.sizeDelta * 0.5f;
    private Canvas canvas;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        ResetGame();
    }

    public void StartMiniGame()
    {
        isActive = true;
        gameObject.SetActive(true);
        ResetGame();
    }

    public void StopMiniGame()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        // Сохраняем предыдущие позиции перед обновлением
        previousPaddlePosition = playerPaddle.anchoredPosition;
        previousAIPosition = aiPaddle.anchoredPosition;

        UpdatePlayerPaddle();
        UpdatePuck();
        UpdateAI();
        CheckGoals();
        UpdateScore();
    }

    // ——— УПРАВЛЕНИЕ ИГРОКОМ ———
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActive) return;
        isDragging = true;
        UpdateTargetPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive || !isDragging) return;
        UpdateTargetPosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    void UpdateTargetPosition(Vector2 screenPoint)
    {
        // Преобразуем координаты экрана в локальные координаты арены
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            arena, screenPoint, canvas.worldCamera, out Vector2 localPoint))
        {
            // Ограничиваем цель ТОЛЬКО левой половиной
            Vector2 clamped = ClampToPlayerZone(localPoint);
            targetPaddlePosition = clamped;
        }
    }

    void UpdatePlayerPaddle()
    {
        // Плавное следование за targetPaddlePosition
        Vector2 currentPos = playerPaddle.anchoredPosition;

        // Используем Lerp для более плавного движения
        Vector2 newPos = Vector2.Lerp(currentPos, targetPaddlePosition, paddleFollowSpeed * Time.deltaTime);
        playerPaddle.anchoredPosition = newPos;
    }

    Vector2 ClampToPlayerZone(Vector2 pos)
    {
        Vector2 half = ArenaHalfSize;
        float paddleWidth = playerPaddle.sizeDelta.x * 0.5f;
        float paddleHeight = playerPaddle.sizeDelta.y * 0.5f;

        // Ограничиваем по X - только левая половина арены
        float minX = -half.x + paddleWidth;
        float maxX = -paddleWidth; // Не заходим за центр

        // Ограничиваем по Y - вся высота арены с отступами
        float minY = -half.y + paddleHeight;
        float maxY = half.y - paddleHeight;

        return new Vector2(
            Mathf.Clamp(pos.x, minX, maxX),
            Mathf.Clamp(pos.y, minY, maxY)
        );
    }

    // ——— ШАЙБА ———
    void UpdatePuck()
    {
        // Двигаем шайбу
        puck.anchoredPosition += puckVelocity * Time.deltaTime;

        Vector2 halfPuck = puck.sizeDelta * 0.5f;
        Vector2 halfArena = ArenaHalfSize;

        // Обработка столкновений с границами
        HandleBoundaryCollisions(halfPuck, halfArena);

        // Ограничение максимальной скорости
        if (puckVelocity.magnitude > maxPuckSpeed)
        {
            puckVelocity = puckVelocity.normalized * maxPuckSpeed;
        }

        // Обработка столкновений с клюшками
        HandlePaddleCollisions(halfPuck);
    }

    void HandleBoundaryCollisions(Vector2 halfPuck, Vector2 halfArena)
    {
        Vector2 puckPos = puck.anchoredPosition;

        // Верхняя и нижняя границы
        if (puckPos.y + halfPuck.y >= halfArena.y)
        {
            puckVelocity.y = -Mathf.Abs(puckVelocity.y) * 0.9f;
            puck.anchoredPosition = new Vector2(puckPos.x, halfArena.y - halfPuck.y);
        }
        else if (puckPos.y - halfPuck.y <= -halfArena.y)
        {
            puckVelocity.y = Mathf.Abs(puckVelocity.y) * 0.9f;
            puck.anchoredPosition = new Vector2(puckPos.x, -halfArena.y + halfPuck.y);
        }

        // Боковые границы (кроме зоны ворот)
        if (!IsInGoalVerticalRange(puckPos.y))
        {
            if (puckPos.x + halfPuck.x >= halfArena.x)
            {
                puckVelocity.x = -Mathf.Abs(puckVelocity.x) * 0.9f;
                puck.anchoredPosition = new Vector2(halfArena.x - halfPuck.x, puckPos.y);
            }
            else if (puckPos.x - halfPuck.x <= -halfArena.x)
            {
                puckVelocity.x = Mathf.Abs(puckVelocity.x) * 0.9f;
                puck.anchoredPosition = new Vector2(-halfArena.x + halfPuck.x, puckPos.y);
            }
        }
    }

    void HandlePaddleCollisions(Vector2 halfPuck)
    {
        float hitRadius = (playerPaddle.sizeDelta.x + puck.sizeDelta.x) * 0.35f;

        // Столкновение с клюшкой игрока
        if (Vector2.Distance(puck.anchoredPosition, playerPaddle.anchoredPosition) < hitRadius &&
            Time.time - lastHitTime > 0.2f)
        {
            Vector2 dir = (puck.anchoredPosition - playerPaddle.anchoredPosition).normalized;

            // Добавляем скорость от движения клюшки
            Vector2 paddleVelocity = (playerPaddle.anchoredPosition - previousPaddlePosition) / Time.deltaTime;
            puckVelocity = dir * maxPuckSpeed + paddleVelocity * 0.3f;

            lastHitTime = Time.time;
        }

        // Столкновение с клюшкой ИИ
        if (Vector2.Distance(puck.anchoredPosition, aiPaddle.anchoredPosition) < hitRadius &&
            Time.time - lastHitTime > 0.2f)
        {
            Vector2 dir = (puck.anchoredPosition - aiPaddle.anchoredPosition).normalized;

            // Добавляем скорость от движения клюшки AI
            Vector2 aiPaddleVelocity = (aiPaddle.anchoredPosition - previousAIPosition) / Time.deltaTime;
            puckVelocity = dir * maxPuckSpeed * 0.85f + aiPaddleVelocity * 0.2f;

            lastHitTime = Time.time;
        }
    }

    bool IsInGoalVerticalRange(float y)
    {
        return Mathf.Abs(y) <= ArenaHalfSize.y * 0.65f;
    }

    // ——— ИИ ———
    void UpdateAI()
    {
        // Предсказываем позицию шайбы
        Vector2 predictedPosition = puck.anchoredPosition + puckVelocity * 0.3f;

        float targetY = Mathf.Clamp(predictedPosition.y, -ArenaHalfSize.y * 0.8f, ArenaHalfSize.y * 0.8f);
        Vector2 targetPos = new Vector2(ArenaHalfSize.x * 0.75f, targetY);

        Vector2 current = aiPaddle.anchoredPosition;
        aiPaddle.anchoredPosition = Vector2.MoveTowards(current, targetPos, paddleFollowSpeed * 0.8f * Time.deltaTime);
    }

    // ——— ГОЛЫ ———
    void CheckGoals()
    {
        Vector2 half = ArenaHalfSize;
        float puckRadius = puck.sizeDelta.x * 0.5f;
        Vector2 puckPos = puck.anchoredPosition;

        // Левые ворота  ИИ забил
        if (puckPos.x + puckRadius < -half.x && IsInGoalVerticalRange(puckPos.y))
        {
            aiScore++;
            ResetRound();
            return;
        }

        // Правые ворота  игрок забил
        if (puckPos.x - puckRadius > half.x && IsInGoalVerticalRange(puckPos.y))
        {
            playerScore++;
            ResetRound();
            return;
        }
    }

    void ResetRound()
    {
        puck.anchoredPosition = Vector2.zero;

        // Задаем случайное направление с преимуществом по X
        Vector2 randomDirection = new Vector2(
            Random.Range(0.3f, 1f) * (Random.value > 0.5f ? 1 : -1),
            Random.Range(-0.7f, 0.7f)
        ).normalized;

        puckVelocity = randomDirection * Random.Range(3f, 6f);

        // Устанавливаем клюшки в начальные позиции
        float offset = ArenaHalfSize.x * 0.75f;
        playerPaddle.anchoredPosition = new Vector2(-offset, 0);
        aiPaddle.anchoredPosition = new Vector2(offset, 0);

        targetPaddlePosition = playerPaddle.anchoredPosition;
        lastHitTime = Time.time;

        // Сбрасываем предыдущие позиции
        previousPaddlePosition = playerPaddle.anchoredPosition;
        previousAIPosition = aiPaddle.anchoredPosition;

        isDragging = false;
    }

    void ResetGame()
    {
        playerScore = 0;
        aiScore = 0;
        ResetRound();
    }

    void UpdateScore()
    {
        scoreText.text = $"{playerScore} : {aiScore}";
    }
}