using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class AIZombieStateMachine : AIStateMachine
{
    #region Serialized fields

    [SerializeField] [Range(10.0f, 360.0f)] float fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float hearing = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int health = 100;
    [SerializeField] [Range(0, 100)] int lowerBodyDamage = 0;
    [SerializeField] [Range(0, 100)] int upperBodyDamage = 0;
    [SerializeField] [Range(0, 100)] int upperBodyThreshold = 30;
    [SerializeField] [Range(0, 100)] int limpThreshold = 30;
    [SerializeField] [Range(0, 100)] int crawlThreshold = 90;
    [SerializeField] [Range(0.0f, 1.0f)] float intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float satisfaction = 1f;
    [SerializeField] float replenishRate = 0.5f;
    [SerializeField] float depletionRate = 0.1f;
    [SerializeField] float reanimationBlendTime = 0.5f;
    [SerializeField] float reanimationWaitTime = 3.0f;
    [SerializeField] LayerMask geometryLayers = 0;
    [SerializeField] [Range(0.0f, 1.0f)] float screamChance = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float screamRadius = 20.0f;
    [SerializeField] AIScreamPosition screamPosition = AIScreamPosition.Entity;
    [SerializeField] AISoundEmitter screamPrefab = null;
    [SerializeField] AudioCollection ragdollCollection = null;

    #endregion

    #region Hashes

    private int speedHash = Animator.StringToHash("Speed");
    private int seekingHash = Animator.StringToHash("Seeking");
    private int feedingHash = Animator.StringToHash("Feeding");
    private int attackHash = Animator.StringToHash("Attack");
    private int crawlHash = Animator.StringToHash("Crawling");
    private int hitTriggerHash = Animator.StringToHash("Hit");
    private int hitTypeHash = Animator.StringToHash("HitType");
    private int reanimateFromBackHash = Animator.StringToHash("Reanimate From Back");
    private int reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
    private int lowerBodyDamageHash = Animator.StringToHash("Lower Body Damage");
    private int upperBodyDamageHash = Animator.StringToHash("Upper Body Damage");
    private int stateHash = Animator.StringToHash("State");
    private int screamingHash = Animator.StringToHash("Screaming");
    private int screamHash = Animator.StringToHash("Scream");

    private int upperBodyLayer = -1;
    private int lowerBodyLayer = -1;

    #endregion

    private AIBoneControlType boneControlType = AIBoneControlType.Animated;
    private List<BodyPartSnapshot> bodyPartSnapshot = new List<BodyPartSnapshot>();
    private float ragdollEndTime = float.MinValue;
    private Vector3 ragdollHipPosition;
    private Vector3 ragdollFeetPosition;
    private Vector3 ragdollHeadPosition;
    private IEnumerator reanimationCoroutine = null;
    private float mecanimTransitionTime = 0.1f;
    private float isScreaming = 0.0f;
    private float nextRagdollSoundTime = 0.0f;


    #region Public properties

    public float Fov { get => fov; }

    public float Hearing { get => hearing; }

    public float Sight { get => sight; }

    public float Intelligence { get => intelligence; }

    public bool Crawling { get; } = false;

    public bool Feeding { get; set; } = false;

    public int AttackType { get; set; } = 0;

    public int Seeking { get; set; } = 0;

    public float Speed { get; set; } = 0.0f;

    public bool IsScreaming
    {
        get
        {
            return isScreaming > 0.1f;
        }
    }

    public float ReplenishRate { get => replenishRate; }

    public float Satisfaction
    {
        get => satisfaction;
        set => satisfaction = value;
    }

    public float Aggression
    {
        get => aggression;
        set => aggression = value;
    }

    public float ScreamChance
    {
        get => screamChance;
    }

    public int Health
    {
        get => health;
        set => health = value;
    }

    public bool IsCrawling
    {
        get { return lowerBodyDamage >= crawlThreshold; }
    }

    #endregion

    public bool Scream()
    {
        if (IsScreaming) return false;

        if (animator == null || IsLayerActive("Cinematic Layer") || screamPrefab == null)
            return false;

        animator.SetTrigger(screamHash);
        Vector3 spawnPosition = screamPosition == AIScreamPosition.Entity ? transform.position : visualThreat.Position;
        AISoundEmitter screamEmitter = Instantiate(screamPrefab, spawnPosition, Quaternion.identity) as AISoundEmitter;

        if (screamEmitter != null)
            screamEmitter.SetRadius(screamRadius);

        return true;
    }

    protected override void Start()
    {
        base.Start();

        if (animator != null)
        {
            lowerBodyLayer = animator.GetLayerIndex("Lower Body");
            upperBodyLayer = animator.GetLayerIndex("Upper Body");
        }

        if (rootBone != null)
        {
            Transform[] transforms = rootBone.GetComponentsInChildren<Transform>();

            foreach (Transform transform in transforms)
            {
                BodyPartSnapshot snapshot = new BodyPartSnapshot();
                snapshot.transform = transform;
                bodyPartSnapshot.Add(snapshot);
            }

        }

        UpdateAnimatorDamage();
    }

    /// <summary>
    /// Refreshes the animator with up-to-date values for its parameters 
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (animator != null)
        {
            animator.SetFloat(speedHash, Speed);
            animator.SetBool(feedingHash, Feeding);
            animator.SetInteger(seekingHash, Seeking);
            animator.SetInteger(attackHash, AttackType);
            animator.SetInteger(stateHash, (int)currentStateType);

            isScreaming = IsLayerActive("Cinematic Layer") ? 0.0f : animator.GetFloat(screamingHash);
        }

        satisfaction = Mathf.Max(0, satisfaction - (depletionRate * Time.deltaTime / 100.0f) * Mathf.Pow(Speed, 3.0f));
    }

    protected void UpdateAnimatorDamage()
    {
        if (animator != null)
        {
            if (lowerBodyLayer != -1)
            {
                animator.SetLayerWeight(lowerBodyLayer, (lowerBodyDamage > limpThreshold && lowerBodyDamage < crawlThreshold) ? 1.0f : 0.0f);
            }

            if (upperBodyLayer != -1)
            {
                animator.SetLayerWeight(upperBodyLayer, (upperBodyDamage > limpThreshold && lowerBodyDamage < crawlThreshold) ? 1.0f : 0.0f);
            }

            animator.SetBool(crawlHash, IsCrawling);
            animator.SetInteger(lowerBodyDamageHash, lowerBodyDamage);
            animator.SetInteger(upperBodyDamageHash, upperBodyDamage);


            if (lowerBodyDamage > limpThreshold && lowerBodyDamage < crawlThreshold)
                SetLayerActive("Lower Body", true);
            else
                SetLayerActive("Lower Body", false);

            if (upperBodyDamage > upperBodyThreshold && lowerBodyDamage < crawlThreshold)
                SetLayerActive("Upper Body", true);
            else
                SetLayerActive("Upper Body", false);
        }
    }

    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager character, int hitDirection)
    {
        float hitStrength = force.magnitude;
        float prevHealth = health;

        if (boneControlType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (Time.time > nextRagdollSoundTime && ragdollCollection != null && health > 0)
                {
                    AudioClip clip = ragdollCollection[1];
                    if (clip && AudioManager.Instance)
                    {
                        nextRagdollSoundTime = Time.time + clip.length;
                        AudioManager.Instance.PlayOneShotSound(ragdollCollection.AudioGroup, clip, position, ragdollCollection.Volume,
                            ragdollCollection.SpatialBlend, ragdollCollection.Priority);
                    }
                }


                if (hitStrength > 1.0f)
                {
                    bodyPart.AddForce(force, ForceMode.Impulse);
                }
                if (bodyPart.CompareTag("Head"))
                {
                    health = Mathf.Max(health - damage, 0);
                }
                else if (bodyPart.CompareTag("Upper Body"))
                {
                    upperBodyDamage += damage;
                }
                else if (bodyPart.CompareTag("Lower Body"))
                {
                    lowerBodyDamage += damage;
                }

                UpdateAnimatorDamage();

                if (health > 0)
                {
                    if (reanimationCoroutine != null)
                        StopCoroutine(reanimationCoroutine);

                    reanimationCoroutine = Reanimate();
                    StartCoroutine(reanimationCoroutine);
                }
            }

            return;
        }

        Vector3 attackerLocPos = transform.InverseTransformPoint(character.transform.position);
        Vector3 hitLocPos = transform.InverseTransformPoint(position);

        bool shouldRagDoll = hitStrength > 1.0f;

        if (bodyPart != null)
        {
            if (bodyPart.CompareTag("Head"))
            {
                health = Mathf.Max(health - damage, 0);
                if (health == 0)
                    shouldRagDoll = true;
            }
            else if (bodyPart.CompareTag("Upper Body"))
            {
                upperBodyDamage += damage;
                UpdateAnimatorDamage();
            }
            else if (bodyPart.CompareTag("Lower Body"))
            {
                lowerBodyDamage += damage;
                UpdateAnimatorDamage();
                shouldRagDoll = true;
            }
        }

        if (boneControlType != AIBoneControlType.Animated || IsCrawling || IsLayerActive("Cinematic Layer") || attackerLocPos.z < 0)
            shouldRagDoll = true;

        if (!shouldRagDoll)
        {
            float angle = 0.0f;

            if (hitDirection == 0)
            {
                Vector3 vecToHit = (position - transform.position).normalized;
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);
            }

            int hitType = 0;

            if (bodyPart.gameObject.CompareTag("Head"))
            {
                if (angle < -10 || hitDirection == -1)
                    hitType = 1;
                else if (angle > 10 || hitDirection == 1)
                    hitType = 3;
                else
                    hitType = 2;
            }
            else if (bodyPart.gameObject.CompareTag("Upper Body"))
            {
                if (angle < -20 || hitDirection == -1)
                    hitType = 4;
                else if (angle > 20 || hitDirection == 1)
                    hitType = 6;
                else
                    hitType = 5;
            }

            if (animator)
            {
                animator.SetInteger(hitTypeHash, hitType);
                animator.SetTrigger(hitTriggerHash);
            }

            return;
        }
        else
        {
            if (currentState)
            {
                currentState.OnExitState();
                currentState = null;
                currentStateType = AIStateType.None;
            }

            if (navAgent) navAgent.enabled = false;
            if (animator) animator.enabled = false;
            if (coll) coll.enabled = false;

            // Mute Audio While Ragdoll is happening
            if (layeredAudioSource != null)
                layeredAudioSource.Mute(true);

            if (Time.time > nextRagdollSoundTime && ragdollCollection != null && prevHealth > 0)
            {
                AudioClip clip = ragdollCollection[0];

                if (clip && AudioManager.Instance)
                {
                    nextRagdollSoundTime = Time.time + clip.length;

                    AudioManager.Instance.PlayOneShotSound(ragdollCollection.AudioGroup, clip, position, ragdollCollection.Volume,
                        ragdollCollection.SpatialBlend, ragdollCollection.Priority);
                }
            }

            InMeleeRange = false;

            foreach (Rigidbody body in bodyParts)
            {
                if (body)
                    body.isKinematic = false;
            }

            if (hitStrength > 1.0f)
            {
                if (bodyPart != null)
                    bodyPart.AddForce(force, ForceMode.Impulse);
            }

            boneControlType = AIBoneControlType.Ragdoll;

            if (health > 0)
            {
                if (reanimationCoroutine != null)
                    StopCoroutine(reanimationCoroutine);

                reanimationCoroutine = Reanimate();
                StartCoroutine(reanimationCoroutine);
            }
        }
    }

    /// <summary>
    /// Starts the reanimation procedure 
    /// </summary>
    protected IEnumerator Reanimate()
    {
        if (boneControlType != AIBoneControlType.Ragdoll || animator == null)
            yield break;

        yield return new WaitForSeconds(reanimationWaitTime);

        ragdollEndTime = Time.time;

        foreach (Rigidbody body in bodyParts)
        {
            body.isKinematic = true;
        }

        boneControlType = AIBoneControlType.RagdollToAnim;

        foreach (BodyPartSnapshot snapShot in bodyPartSnapshot)
        {
            snapShot.position = snapShot.transform.position;
            snapShot.rotation = snapShot.transform.rotation;
        }

        ragdollHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
        ragdollFeetPosition = (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;
        ragdollHipPosition = rootBone.position;

        animator.enabled = true;

        if (rootBone != null)
        {
            float forwardTest;

            switch (rootBoneAlingment)
            {
                case AIBoneAlignmentType.ZAxis:
                    forwardTest = rootBone.forward.y; break;
                case AIBoneAlignmentType.ZAxisInverted:
                    forwardTest = -rootBone.forward.y; break;
                case AIBoneAlignmentType.YAxis:
                    forwardTest = rootBone.up.y; break;
                case AIBoneAlignmentType.YAxisInverted:
                    forwardTest = -rootBone.up.y; break;
                case AIBoneAlignmentType.XAxis:
                    forwardTest = rootBone.right.y; break;
                case AIBoneAlignmentType.XAxisInverted:
                    forwardTest = -rootBone.right.y; break;
                default:
                    forwardTest = rootBone.forward.y; break;
            }

            if (forwardTest >= 0)
                animator.SetTrigger(reanimateFromBackHash);
            else
                animator.SetTrigger(reanimateFromFrontHash);
        }

        yield break;
    }

    protected virtual void LateUpdate()
    {
        if (boneControlType == AIBoneControlType.RagdollToAnim)
        {
            if (Time.time <= ragdollEndTime + mecanimTransitionTime)
            {
                Vector3 animatedToRagdoll = ragdollHipPosition - rootBone.position;
                Vector3 newRootPosition = transform.position + animatedToRagdoll;

                RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, geometryLayers);
                newRootPosition.y = float.MinValue;

                foreach (RaycastHit hit in hits)
                {
                    if (!hit.transform.IsChildOf(transform))
                    {
                        newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);
                    }
                }

                NavMeshHit navMeshHit;

                Vector3 baseOffset = Vector3.zero;
                if (navAgent) baseOffset.y = navAgent.baseOffset;

                if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 25.0f, NavMesh.AllAreas))
                {
                    transform.position = navMeshHit.position + baseOffset;
                }
                else
                {
                    transform.position = newRootPosition + baseOffset;
                }

                Vector3 ragdollDirection = ragdollHeadPosition - ragdollFeetPosition;
                ragdollDirection.y = 0.0f;

                Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0.0f;

                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDirection.normalized);
            }

            float blendAmount = Mathf.Clamp01((Time.time - ragdollEndTime - mecanimTransitionTime) / reanimationBlendTime);

            // Calculate blended bone position by interpolating between ragdoll bone snapshot and animated bone positions
            foreach (BodyPartSnapshot snapshot in bodyPartSnapshot)
            {
                if (snapshot.transform == rootBone)
                {
                    snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);
                }

                snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);

            }

            if (blendAmount == 1.0f)
            {
                boneControlType = AIBoneControlType.Animated;

                if (navAgent)
                    navAgent.enabled = true;
                if (coll)
                    coll.enabled = true;

                AIState newState = null;
                if (states.TryGetValue(AIStateType.Alerted, out newState))
                {
                    if (currentState != null)
                        currentState.OnExitState();
                    newState.OnEnterState();
                    currentState = newState;
                    currentStateType = AIStateType.Alerted;
                }
            }
        }
    }
}
