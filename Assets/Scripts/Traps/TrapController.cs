using UnityEngine;

/// <summary>
/// Handles the state and the animations of the traps.
/// Traps alternate between active and inactive state
/// </summary>
public class TrapController : MonoBehaviour
{
    private Animator _animator;

    private bool isTrapActive = false;

    private float timeSinceTrapSwitch = 0;
    [SerializeField] private float trapSwitchTime = 1;
    
    private const string _spikesOut = "SpikesOut";

    public bool IsTrapActive
    {
        get { return this.isTrapActive; }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Tracks when is it time for the trap to switch from active to inactive and vice versa
    /// </summary>
    void Update()
    {
        timeSinceTrapSwitch += Time.deltaTime;

        if (timeSinceTrapSwitch >= trapSwitchTime)
        {
            SwitchTrap();
            _animator.SetBool(_spikesOut, isTrapActive);
            timeSinceTrapSwitch = 0f;
        }
    }

    /// <summary>
    /// Switches the trap status
    /// </summary>
    private void SwitchTrap()
    {
        if (isTrapActive) isTrapActive = false;
        else isTrapActive = true;
    }
}
