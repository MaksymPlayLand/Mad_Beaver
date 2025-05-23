
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Control : MonoBehaviour

{
    private Input input;

    private bool isMoveLeft = false;
    private bool isMoveRight = false;
    //private bool isJumping;
    private bool isGrounded = false;
    public bool isWater = false;

    public Animator animator;

    public GameObject gameOverMenu;

    public GameObject Player;
    public GameObject screenController;
    private bool isTouch = false;

    private bool isGameOver = false;

    private Rigidbody2D rb;
    public float jumpForce = 5f;
    public float pauseTime = 5f;

    public float gravity = 1f;
    private float defaultGravity;

    public GameObject stickPrefab; // Префаб камінчика
    public Transform throwPoint;  // Точка, з якої кидатиметься камінчик
    public float throwForce = 10f; // Сила кидка
                                   //  private bool IsStickFly;
    private int groundContacts = 0;
    public Text firewoodText;

    private float previousY;
    private float addForce = 0f;



    private float lastThrowTime = 0f;
    private float throwCooldown = 0.3f; // Час між кидками

    private Vector2 previousPosition;
    public ParticleSystem bubbleSystem; // Посилання на систему частинок
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    // Start is called before the first frame update
    void Awake()

    {
        input = new Input();

        input.player.Left.performed += moveLeft;
        input.player.Left.canceled += stopLeft;
        input.player.Right.performed += moveRight;
        input.player.Right.canceled += stopRight;
        input.player.Jump.performed += onJump;
        input.player.Down.performed += onDown;
        input.player.Down.canceled += onStopDown;
        input.player.AngleJump.performed += onAngleJump;
        input.player.Throw.performed += stickFly;
        // input.player.Throw.canceled += stickNoFly;



    }

    private void Start()
    {
        SoundManager.Instance.StopEffectsSound();
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;

        animator = GetComponent<Animator>();

        previousY = transform.position.y;

        // audioSource.PlayOneShot(startSound);
        GlobalResources.Firewood = 0;

        SoundManager.Instance.PlayBackgroundMusic(SoundManager.Instance.backgroundSound);

        // Отримуємо доступ до Velocity over Lifetime модуля
        velocityModule = bubbleSystem.velocityOverLifetime;
        previousPosition = transform.position;

        Debug.Log("touchScreen is: (for start)" + TouchControlsManager.Instance.IsTouch);
    }


    // Update is called once per frame
    private void Update()
    {
        if (isMoveLeft)
        {
            transform.position += Vector3.left * 5 * Time.deltaTime;

            if (isGrounded)
            {
                animator.SetBool("IsGo", true);
                SoundManager.Instance.PlayWalkSound();
            }

        }


        if (isMoveRight)
        {
            transform.position += Vector3.right * 5 * Time.deltaTime;

            if (isGrounded)
            {
                animator.SetBool("IsGo", true);
                SoundManager.Instance.PlayWalkSound();
            }
        }
        if (!isWater)
        {
            // Зчитування поточної позиції по осі Y
            float currentY = transform.position.y;

            // Перевірка напрямку переміщення
            if (currentY > previousY + 0.001f)
            {
                addForce = 2f;
            }
            else if (currentY < previousY - 0.001f)
            {
                addForce = -2f;
            }

            else
            {
                addForce = 0f;
            }

            // Оновлення попередньої позиції
            previousY = currentY;

        }

        if (isWater)
        {
            // Обчислюємо швидкість персонажа (напрямок руху)
            Vector2 currentPosition = transform.position;
            Vector2 velocity = (currentPosition - previousPosition) / Time.deltaTime;

            previousPosition = currentPosition;

            // Використовуємо горизонтальну швидкість для зміни orbitalY
            velocityModule.x = velocity.x * -1f; // -4f для масштабування ефекту
        }
    }

    private void OnEnable()
    {
        input.Enable();

        Debug.Log("touchScreen is: (for enable)" + TouchControlsManager.Instance.IsTouch);

        if (TouchControlsManager.Instance != null && TouchControlsManager.Instance.IsTouch)
        {
            screenController.SetActive(true);
            Debug.Log("touchScreen is (for true): " + TouchControlsManager.Instance.IsTouch);
        }
        else
        {
            screenController.SetActive(false);
            Debug.Log("touchScreen is: (for false)" + TouchControlsManager.Instance.IsTouch);
        }
    }

    private void OnDisable()
    {
        input.Disable();
        if (screenController)
        {
            screenController.SetActive(false);
        }

    }

    private void moveLeft(InputAction.CallbackContext context)
    {
        isMoveLeft = true;
        Vector3 scale = transform.localScale;
        scale.x = -0.25f; // Змінюємо знак по осі X
        transform.localScale = scale;

        //  animator.SetBool("IsJump", false);
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }
    private void stopLeft(InputAction.CallbackContext context)
    {
        isMoveLeft = false;
        animator.SetBool("IsGo", false);
        // audioSource.Stop();
        SoundManager.Instance.StopWalkSound();

    }

    private void moveRight(InputAction.CallbackContext context)
    {
        isMoveRight = true;
        Vector3 scale = transform.localScale;
        scale.x = 0.25f; // Змінюємо знак по осі X
        transform.localScale = scale;
        // animator.SetBool("IsJump", false);
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void stopRight(InputAction.CallbackContext context)
    {
        isMoveRight = false;
        animator.SetBool("IsGo", false);
        // audioSource.Stop();
        SoundManager.Instance.StopWalkSound();
    }


    public void onJump(InputAction.CallbackContext context)
    {
        if (isWater || isGrounded) // Only jump if grounded
        {

            rb.velocity = new Vector2(rb.velocity.x, jumpForce + addForce);

            // audioSource.Stop();
            // audioSource.PlayOneShot(jumpSound);

            // Зупиняємо звук кроків перед стрибком
            SoundManager.Instance.StopWalkSound();


            // Відтворюємо звук стрибка
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.jumpSound);

            Debug.Log("Force is: " + rb.velocity);
        }
    }

    public void onDown(InputAction.CallbackContext context)
    {
        if (isWater)
        {
            rb.gravityScale = gravity;

            animator.SetBool("IsSwim", false);

            // Зупиняємо звук кроків перед стрибком
            SoundManager.Instance.StopWalkSound();


            // Відтворюємо звук стрибка
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.jumpSound);
        }
    }

    public void onStopDown(InputAction.CallbackContext context)
    {
        if (isWater)
            rb.gravityScale = defaultGravity;
        animator.SetBool("IsSwim", true);
    }

    public void onAngleJump(InputAction.CallbackContext context)
    {
        if (isWater || isGrounded) // Only jump if grounded
        {
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            rb.velocity = new Vector2(jumpForce * 0.6f * direction, (jumpForce + addForce) * 1.2f);
            // audioSource.Stop();
            // audioSource.PlayOneShot(jumpSound);

            // Зупиняємо звук кроків перед стрибком
            SoundManager.Instance.StopWalkSound();

            // Відтворюємо звук стрибка
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.jumpSound);

            Debug.Log("Force is: " + rb.velocity);
        }
    }



    public void stickFly(InputAction.CallbackContext context)
    {
        StartCoroutine(ThrowAnimation());
        if (GlobalResources.Firewood > 0)
        {
            //  audioSource.PlayOneShot(hitSound);
            if (Time.time - lastThrowTime < throwCooldown)
                return; // Якщо ще не минуло 0.5 секунди, виходимо

            lastThrowTime = Time.time; // Оновлюємо час останнього кидка
            // animator.SetBool("IsThrow", true);

            // Створюємо stick у точці кидка
            GameObject stick = Instantiate(stickPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody2D rb = stick.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Визначаємо напрямок кидка залежно від напряму персонажа
                float direction = transform.localScale.x > 0 ? 1f : -1f;
                rb.velocity = new Vector2(throwForce * direction, 0);

                if (isWater)
                {
                    // Додаємо обертання кулі під час її руху
                    rb.angularVelocity = -300f;
                }

                GlobalResources.Firewood -= 1;
                firewoodText.text = "" + GlobalResources.Firewood;

                SoundManager.Instance.PlayOneShot(SoundManager.Instance.flyStickSound);

            }
        }
        else
        {
            firewoodText.text = "X";
            //  audioSource.PlayOneShot(hitFailSound);
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.emptyStickSound);

        }
    }

    public IEnumerator ThrowAnimation()
    {
        animator.SetBool("IsThrow", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("IsThrow", false);
    }

    // public void stickNoFly(InputAction.CallbackContext context)
    // {
    //     animator.SetBool("IsThrow", false);
    // }

    public void OnPause()
    {
        //animator.SetBool("IsThrow", false);
        animator.SetBool("pause", true);
    }

    public void EndPause()
    {
        animator.SetBool("pause", false);

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts++;
            if (groundContacts == 1) // Якщо це перший контакт із землею, то вважаємо персонажа приземленим
            {
                isGrounded = true;
                animator.SetBool("IsJump", false);

            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {


        if (collision.gameObject.CompareTag("Ground") && !isGameOver)
        {
            groundContacts--;
            if (groundContacts <= 0) // Якщо всі контакти зникли, то персонаж у повітрі
            {
                isGrounded = false;
                if (!isWater)
                {
                    animator.SetBool("IsJump", true);
                }
                else if (isWater)
                {
                    animator.SetBool("IsSwim", true);
                }
                animator.SetBool("IsGo", false);
                SoundManager.Instance.StopWalkSound();

            }
        }


    }

    public void TriggerGameOver()
    {
        if (!isGameOver)
        {
            screenController.SetActive(false);
            // SoundManager.Instance.StopEffectsSound();
            isGameOver = true;
            isGrounded = true;
            animator.SetBool("IsSwim", false);

            animator.SetBool("IsDead", true);
            //SoundManager.Instance.PlayOneShot(SoundManager.Instance.deathSound);

            // Додатково: зупинити рух або інші дії персонажа
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            // GetComponent<PlayerMovement>().enabled = false; // Якщо є скрипт руху

            // Скидання життів до 3
            HealthBar.life = 3; // Оновлення статичної змінної
            StartCoroutine(GameOverMenu());
        }
    }



    public IEnumerator GameOverMenu()
    {
        screenController.SetActive(false);
        yield return new WaitForSeconds(3f);
        // Зупиняємо звуки
        SoundManager.Instance.StopEffectsSound();
        SoundManager.Instance.StopBackgroundSound();

        gameOverMenu.SetActive(true);
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.gameOverSound);
        Player.SetActive(false);

    }

    public void RestartGame()
    {
        SoundManager.Instance.StopEffectsSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel(int level)
    {
        SoundManager.Instance.StopEffectsSound();
        SceneManager.LoadScene(level);
        FindObjectOfType<GameMenus>().LevelCompleted(level);
    }

    public void ExitGame()
    {
        // Закриваємо гру (працює тільки у збірці)
        Application.Quit();
        // Для редактора:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void TouchLeft()
    {
        var fakeContext = new InputAction.CallbackContext();
        moveLeft(fakeContext);
    }

    public void TouchRight()
    {
        var fakeContext = new InputAction.CallbackContext();
        moveRight(fakeContext);
    }

    public void UnTouchLeft()
    {
        var fakeContext = new InputAction.CallbackContext();
        stopLeft(fakeContext);
    }

    public void UnTouchRight()
    {
        var fakeContext = new InputAction.CallbackContext();
        stopRight(fakeContext);
    }

    public void TouchJump()
    {
        var fakeContext = new InputAction.CallbackContext();
        onJump(fakeContext);
    }

    public void TouchThrow()
    {
        var fakeContext = new InputAction.CallbackContext();
        stickFly(fakeContext);
    }
}