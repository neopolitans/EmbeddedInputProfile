/*                                        Embedded Input Profile by Alexander "maylizbeth" Gilbertson / B512325
 *                                        
 *                                    More information can be found in the GitHub. PROVIDED UNDER THE MIT LICENSE
 *                                                  https://github.com/neopolitans/EmbeddedInputProfile     
 *                                                  
 *------------------------------------------------------------------------------------------------------------------------------------------------------------
 *                                                          INPUT PROFILE MODULE for UNITY ENGINE
 *------------------------------------------------------------------------------------------------------------------------------------------------------------
 *
 *  An Optional Module for developers to create rebindable actions and contain them in profiles, objects that represent platforms that a game is being 
 *  developed for. However, developers can also use all abstract classes seoarately and completely decouple this module from EmbeddedInputModule for a 
 *  preferred input handler (e.g. Rewired, Unity.Input, InputSystem 1.7.0).
 *
 *
 *                         OPTIONAL FEATURES OF THIS MODULE REQUIRE EmbeddedInputModule AND UnityEngine.InputSystem package to work!
 *                                    More information can be found in the GitHub. PROVIDED UNDER THE MIT LICENSE
 *                                                  https://github.com/neopolitans/EmbeddedInputModule
 */

//------------------------------------------------------------------------------------------------------------------------------------------------------------
//                                                                 INPUT PROFILE | SETTINGS
//------------------------------------------------------------------------------------------------------------------------------------------------------------

//#define EIM_OPTMOD_InputProfile_DecoupleFromEmbeddedInputModule             // Masks all EmbeddedInputModule Dependent References and Classes.
                                                                              // Provided to make using all core classes possible outside of EIM.

//------------------------------------------------------------------------------------------------------------------------------------------------------------
// P.S. This is the second rewrite to incorporate decoupling. The original was written for UnityEngine.Input and then first rewritten for InputSystem + EIM.


using UnityEngine;

#if !EIM_OPTMOD_InputProfile_DecoupleFromEmbeddedInputModule
using Input = EmbeddedInputModule;
using GamepadControl = EmbeddedInputModule.GamepadControl;
#endif

/// <summary>
/// A list of available Input Axes.
/// </summary>
[System.Serializable]
public enum AvailableInputAxes
{
    LeftStick,
    RightStick,
    MouseAxis,
    ButtonAxis
}

/// <summary>
/// An interface containing the base definitions of Held, Pressed and Released.<br/>
/// so that virtual buttons can be identified and read from in an InputProfile.
/// </summary>
internal interface IDeviceInputButton
{
    public bool Held { get; }
    public bool Pressed { get; }
    public bool Released { get; }
}

/// <summary>
/// Abstract Class containing all core members for any virtual input.
/// </summary>
[System.Serializable]
public abstract class AbstractContextualAction
{
    // Members
    /// <summary>
    /// The method(s) to trigger when this action is rebound.
    /// </summary>
    public System.Action onRebind = null;

    /// <summary>
    /// The label of this action. <br/>
    /// Used to query for actions.
    /// </summary>
    public string label { get; protected set; } = "";

    /// <summary>
    /// Is this action a binding for an Input Axis?
    /// </summary>
    public bool isAxis => GetType() == typeof(BaseContextualAxis);

    /// <summary>
    /// Is this action a binding for a Virtual Button?
    /// </summary>
    public bool isButton => GetType() == typeof(IDeviceInputButton);

    /// <summary>
    /// Creates a new instance of the action with the same data provided for the original object.
    /// </summary>
    /// <returns></returns>
    public abstract AbstractContextualAction Clone();
}

/// <summary>
/// Abstract Class containing inputSource, a generic variable used to determine where input is polled from.
/// </summary>
/// <typeparam name="TEnum">
/// The Enumerator representing the expected input source. <br/> 
/// This should be a Key Code, Available Input Axis or similar-purpose enumerator.
/// </typeparam>
[System.Serializable]
public abstract class BaseContextualAction<TEnum> : AbstractContextualAction where TEnum : System.Enum
{
    // This is going to be trickier to work with, but probably far more worth it in the long run.

    /// <summary>
    /// The input source. <br/>
    /// This can be a Key Code, Available Input Axis or other representation.   
    /// </summary>
    public TEnum inputSource
    {
        get { return m_inputSource; }
        set
        {
            m_inputSource = value;
            if (onRebind != null)
            {
                onRebind();
            }
        }
    }

    /// <summary>
    /// [Internal] The internally wrapped input source value accessed by the property.
    /// </summary>
    private TEnum m_inputSource;
}

/// <summary>
/// Abstract Class implementing additional properties for any virtual button.
/// </summary>
/// <typeparam name="TEnum">
/// The Enumerator representing the expected input source. <br/> 
/// This should be a Key Code or similar-purpose enumerator.
/// </typeparam>
[System.Serializable]   
public abstract class BaseContextualButton<TEnum> : BaseContextualAction<TEnum>, IDeviceInputButton where TEnum : System.Enum
{
    /// <summary>
    /// Whether the button is pressed.
    /// </summary>
    public abstract bool Pressed { get; }

    /// <summary>
    /// Whether the button is held.
    /// </summary>
    public abstract bool Held { get; }

    /// <summary>
    /// Whether the button is released.
    /// </summary>
    public abstract bool Released { get; }
}

/// <summary>
/// Abstract Class containing a Vector2 property, Value, for any virtual axis.
/// </summary>
[System.Serializable]
public abstract class BaseContextualAxis : BaseContextualAction<AvailableInputAxes>
{
    // A class was needed that held a Vector2 Value to get any input motions from an input axis.

    /// <summary>
    /// The value being read from the Input Axis.
    /// </summary>
    public abstract Vector2 Value { get; }
}

/// <summary>
/// A class that contains a list of virtual inputs for player actions . <br/>
/// Suitable for collections in arrays and lists.
/// </summary>
[System.Serializable]
public class InputProfile
{
    // Members
    /// <summary>
    /// The Actions contained in this Input Profile
    /// </summary>
    public AbstractContextualAction[] actions = new AbstractContextualAction[0];

    // Constructors
    /// <summary>
    /// Create an InputProfile
    /// </summary>
    /// <param name="actionList"></param>
    public InputProfile(params AbstractContextualAction[] actionList)
    {
        actions = actionList;
    }

    // Methods
    /// <summary>
    /// Returns true while the virtual button identified by buttonName is held down.
    /// </summary>
    /// <param name="buttonName">The name of the Virtual Button.</param>
    /// <returns></returns>
    public bool GetButton(string buttonName)
    {
        if (actions.Length < 1) return false;

        foreach (AbstractContextualAction action in actions)
        {
            if (action.label == buttonName)
            {
                return ((IDeviceInputButton)action).Held;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true during the first frame the user pressed down the virtual button idetnified by buttonName.
    /// </summary>
    /// <param name="buttonName">The name of the Virtual Button.</param>
    /// <returns></returns>
    public bool GetButtonDown(string buttonName)
    {
        if (actions.Length < 1) return false;

        foreach (AbstractContextualAction action in actions)
        {
            if (action.label == buttonName)
            {
                return ((IDeviceInputButton)action).Pressed;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true during the first frame the user releases the virtual button idetnified by buttonName.
    /// </summary>
    /// <param name="buttonName">The name of the Virtual Button.</param>
    /// <returns></returns>
    public bool GetButtonUp(string buttonName)
    {
        if (actions.Length < 1) return false;

        foreach (AbstractContextualAction action in actions)
        {
            if (action.label == buttonName)
            {
                return ((IDeviceInputButton)action).Released;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the value of the virtual axis identified by axisName. <br/>
    /// The value will be in the range of -1 to 1 for keyboard and joystick input devices.
    /// </summary>
    /// <param name="axisName">The name of the Virtual Axis.</param>
    /// <returns></returns>
    public Vector2 GetAxis(string axisName)
    {
        if (actions.Length < 1) return Vector2.zero;

        foreach (AbstractContextualAction action in actions)
        {
            if (action.label == axisName)
            {
                return ((BaseContextualAxis)action).Value;
            }
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Duplicates all underlying Actions and returns a new instance with all action info.
    /// </summary>
    /// <returns><see cref="InputProfile"/></returns>
    public virtual InputProfile Clone()
    {
        AbstractContextualAction[] duplicateActions = new AbstractContextualAction[actions.Length];

        for (int i = 0; i < duplicateActions.Length; i++)
        {
            duplicateActions[i] = actions[i].Clone();
        }

        return new InputProfile(duplicateActions);
    }
}

#if !EIM_OPTMOD_InputProfile_DecoupleFromEmbeddedInputModule

/// <summary>
/// A virtual button that represents a key on a Keyboard, Mouse or Gamepad.
/// </summary>
[System.Serializable]
public class ContextualKey : BaseContextualButton<KeyCode>
{
    // Members
    /// <summary>
    /// An alternate input source to fetch input for.
    /// </summary>
    protected KeyCode m_altInputSource;

    // Properties

    /// <summary>
    /// The alternate Input Source, if an alternate is desired.
    /// </summary>
    public KeyCode altInputSource
    {
        get {  return m_altInputSource; } 
        set 
        {
            m_altInputSource = value; 
            if (onRebind != null) { onRebind(); }
        }
    }

    public override bool Held
    {
        get { return Input.GetKey(inputSource) || Input.GetKey(altInputSource); }
    }
    public override bool Pressed
    {
        get { return Input.GetKeyDown(inputSource) || Input.GetKeyDown(altInputSource); }
    }
    public override bool Released
    {
        get { return Input.GetKeyUp(inputSource) || Input.GetKeyUp(altInputSource); }
    }
    
    // Constructors
    // - There aren't GamepadControl versions of these constructors as this class has very specific uses.
    // - GamepadControl enum is used by ContextualGamepadButton.

    /// <summary>
    /// Create a Virtual Button that represents a control on a keyboard, mouse or gamepad.
    /// </summary>
    /// <param name="key">The key bound to the virtual button.</param>
    public ContextualKey(string buttonName, KeyCode key)
    {
        inputSource = key;
        label = buttonName;
    }

    /// <summary>
    /// Create a Virtual Button that represents a control on a keyboard, mouse or gamepad.<br/>
    /// Contains a callback delegate which is called on any input being Rebound.
    /// </summary>
    /// <param name="control"></param>
    public ContextualKey(string buttonName, KeyCode key, System.Action onRebind)
    {
        inputSource = key;
        label = buttonName;
        this.onRebind = onRebind;
    }

    /// <summary>
    /// Create a Virtual Button that represents a primary and alternate control on a keyboard, mouse or gamepad.
    /// </summary>
    /// <param name="key">The key bound to the virtual button.</param>
    public ContextualKey(string buttonName, KeyCode key, KeyCode altKey)
    {
        inputSource = key;
        altInputSource = altKey;
        label = buttonName;
    }

    /// <summary>
    /// Create a Virtual Button that represents a primary and alternate control on a keyboard, mouse or gamepad.<br/>
    /// Contains a callback delegate which is called on any input being Rebound.
    /// </summary>
    /// <param name="control"></param>
    public ContextualKey(string buttonName, KeyCode key, KeyCode altKey, System.Action onRebind)
    {
        inputSource = key;
        altInputSource = altKey;
        label = buttonName;
        this.onRebind = onRebind;
    }

    // Methods
    public override AbstractContextualAction Clone()
    {
        return new ContextualKey(label, inputSource, altInputSource, onRebind);
    }
}

/// <summary>
/// A virtual button that represents a control on a Gamepad.
/// </summary>
[System.Serializable]
public class ContextualGamepadButton : BaseContextualButton<GamepadControl>
{
    // Properties
    public override bool Held
    {
        get { return Input.GetKey(Input.GamepadControlToKeyCode(inputSource)); }
    }

    public override bool Pressed
    {
        get { return Input.GetKeyDown(Input.GamepadControlToKeyCode(inputSource)); }
    }

    public override bool Released
    {
        get { return Input.GetKeyUp(Input.GamepadControlToKeyCode(inputSource)); }
    }

    // Constructors
    /// <summary>
    /// Create a Virtual Button that represents a control on a Gamepad.
    /// </summary>
    /// <param name="control">The button on the gamepad to be assigned to the action.</param>
    public ContextualGamepadButton(string buttonName, GamepadControl control)
    {
        inputSource = control;
        label = buttonName;
    }

    /// <summary>
    /// Create a Virtual Button that represents a control on a Gamepad.<br/>
    /// Contains a callback delegate which is called on any input being Rebound.
    /// </summary>
    /// <param name="control"></param>
    public ContextualGamepadButton(string buttonName, GamepadControl control, System.Action onRebind)
    {
        inputSource = control;
        label = buttonName;
        this.onRebind = onRebind;
    }

    // Methods
    public override AbstractContextualAction Clone()
    {
        return new ContextualGamepadButton(label, inputSource, onRebind);
    }
}

/// <summary>
/// A virtual axis that represents a multi-directional control through a Joystick, Mouse or set of 4 buttons.
/// </summary>
[System.Serializable]
public class ContextualAxis : BaseContextualAxis
{
    // Members
    protected KeyCode m_positiveX = KeyCode.None;
    protected KeyCode m_negativeX = KeyCode.None;
    protected KeyCode m_positiveY = KeyCode.None;
    protected KeyCode m_negativeY = KeyCode.None;
    protected KeyCode m_altPositiveX = KeyCode.None;
    protected KeyCode m_altNegativeX = KeyCode.None;
    protected KeyCode m_altPositiveY = KeyCode.None;
    protected KeyCode m_altNegativeY = KeyCode.None;

    // Properties
    public KeyCode positiveX
    {
        get { return m_positiveX; }
        set
        {
            m_positiveX = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode negativeX
    {
        get { return m_negativeX; }
        set
        {
            m_negativeX = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode positiveY
    {
        get { return m_positiveY; }
        set
        {
            m_positiveY = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode negativeY
    {
        get { return m_negativeY; }
        set
        {
            m_negativeY = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode altPositiveX
    {
        get { return m_altPositiveX; }
        set
        {
            m_altPositiveX = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode altNegativeX
    {
        get { return m_altNegativeX; }
        set
        {
            m_altNegativeX = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode altPositiveY
    {
        get { return m_altPositiveY; }
        set
        {
            m_altPositiveY = value;
            if (onRebind != null) onRebind();
        }
    }
    public KeyCode altNegativeY
    {
        get { return m_altNegativeY; }
        set
        {
            m_altNegativeY = value;
            if (onRebind != null) onRebind();
        }
    }

    public override Vector2 Value
    {
        get
        {
            switch (inputSource)
            {
                default:
                    return Input.LeftStick;

                case AvailableInputAxes.RightStick:
                    return Input.RightStick;

                case AvailableInputAxes.MouseAxis:
                    return Input.mouseDelta;

                case AvailableInputAxes.ButtonAxis:
                    float x = 0f, y = 0f;

                    bool pXActive = Input.GetKeyDown(m_positiveX) || Input.GetKey(m_positiveX) || Input.GetKeyDown(m_altPositiveX) || Input.GetKey(m_altPositiveX);
                    bool nXActive = Input.GetKeyDown(m_negativeX) || Input.GetKey(m_negativeX) || Input.GetKeyDown(m_altNegativeX) || Input.GetKey(m_altNegativeX);
                    bool pYActive = Input.GetKeyDown(m_positiveY) || Input.GetKey(m_positiveY) || Input.GetKeyDown(m_altPositiveY) || Input.GetKey(m_altPositiveY);
                    bool nYActive = Input.GetKeyDown(m_negativeY) || Input.GetKey(m_negativeY) || Input.GetKeyDown(m_altNegativeY) || Input.GetKey(m_altNegativeY);

                    if (pXActive) x += 1.0f;
                    if (nXActive) x -= 1.0f;
                    if (pYActive) y += 1.0f;
                    if (nYActive) y -= 1.0f;

                    return new Vector2(x, y).normalized;
            }
        }
    }
    
    // Constructors
    /// <summary>
    /// Create a Virtual Axis that reads from a specific joystick axis. <br/><br/>
    /// For Button Axes, use the alternate constructors: <br/>
    /// <see cref="ContextualAxis.ContextualAxis(string, KeyCode, KeyCode, KeyCode, KeyCode)"/><br/>
    /// <see cref="ContextualAxis.ContextualAxis(string, GamepadControl, GamepadControl, GamepadControl, GamepadControl)"/>
    /// </summary>
    /// <param name="label">The label of the Contextual Axis</param>
    /// <param name="axis"></param>
    public ContextualAxis(string label, AvailableInputAxes axis)
    {
        // Set the input source based on the button axis
        inputSource = axis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        if (axis is AvailableInputAxes.ButtonAxis)
        {
            Debug.LogWarning("[ContextualAxis] ButtonAxis has no keybinds.\nUse ContextualAxis(string, KeyCode, KeyCode, KeyCode, KeyCode) for a pre-set keybind.");
        }
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four keys for it's positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    /// <param name="label"></param>
    /// <param name="positiveX"></param>
    /// <param name="negativeX"></param>
    /// <param name="positiveY"></param>
    /// <param name="negativeY"></param>
    public ContextualAxis(string label, KeyCode positiveX, KeyCode negativeX, KeyCode positiveY, KeyCode negativeY)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Assign all buttons to the respective directions.
        m_positiveX = positiveX;
        m_negativeX = negativeX;
        m_positiveY = positiveY;
        m_negativeY = negativeY;
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four gamepad buttons for it's positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    /// <param name="label"></param>
    /// <param name="positiveX"></param>
    /// <param name="negativeX"></param>
    /// <param name="positiveY"></param>
    /// <param name="negativeY"></param>
    public ContextualAxis(string label, GamepadControl positiveX, GamepadControl negativeX, GamepadControl positiveY, GamepadControl negativeY)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Assign all buttons to the respective directions.
        m_positiveX = Input.GamepadControlToKeyCode(positiveX);
        m_negativeX = Input.GamepadControlToKeyCode(negativeX);
        m_positiveY = Input.GamepadControlToKeyCode(positiveY);
        m_negativeY = Input.GamepadControlToKeyCode(negativeY);
    }
    
    /// <summary>
    /// Create a Virtual Axis that reads from a specific joystick axis. <br/>
    /// Contains a callback delegate which is called on any input being Rebound.<br/><br/>
    /// For Button Axes, use the alternate constructors: <br/>
    /// <see cref="ContextualAxis.ContextualAxis(string, KeyCode, KeyCode, KeyCode, KeyCode)"/><br/>
    /// <see cref="ContextualAxis.ContextualAxis(string, GamepadControl, GamepadControl, GamepadControl, GamepadControl)"/>
    /// </summary>
    /// <param name="label">The label of the Contextual Axis</param>
    /// <param name="axis">The axis to bind to.</param>
    public ContextualAxis(string label, AvailableInputAxes axis, System.Action onRebind)
    {
        // Set the input source based on the button axis
        inputSource = axis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Set OnRebind Callback.
        this.onRebind = onRebind;

        if (axis is AvailableInputAxes.ButtonAxis)
        {
            Debug.LogWarning("[ContextualAxis] ButtonAxis has no keybinds.\nUse ContextualAxis(string, KeyCode, KeyCode, KeyCode, KeyCode) for a pre-set keybind.");
        }
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four keys for it's positive X/Y and negative X/Y values.<br/>
    /// Contains a callback delegate which is called on any input being Rebound.<br/><br/>
    /// For Joystick axes, use the alternate constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, KeyCode positiveX, KeyCode negativeX, KeyCode positiveY, KeyCode negativeY, System.Action onRebind)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Set OnRebind Callback.
        this.onRebind = onRebind;

        // Assign all buttons to the respective directions.
        m_positiveX = positiveX;
        m_negativeX = negativeX;
        m_positiveY = positiveY;
        m_negativeY = negativeY;
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four gamepad buttons for it's positive X/Y and negative X/Y values.<br/>
    /// Contains a callback delegate which is called on any input being Rebound.<br/><br/>
    /// For Joystick axes, use the alternate constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, GamepadControl positiveX, GamepadControl negativeX, GamepadControl positiveY, GamepadControl negativeY, System.Action onRebind)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Set OnRebind Callback.
        this.onRebind = onRebind;

        // Assign all buttons to the respective directions.
        m_positiveX = Input.GamepadControlToKeyCode(positiveX);
        m_negativeX = Input.GamepadControlToKeyCode(negativeX);
        m_positiveY = Input.GamepadControlToKeyCode(positiveY);
        m_negativeY = Input.GamepadControlToKeyCode(negativeY);
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four keys for either it's primary or secondary positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, KeyCode positiveX, KeyCode negativeX, KeyCode positiveY, KeyCode negativeY, KeyCode altPositiveX, KeyCode altNegativeX, KeyCode altPositiveY, KeyCode altNegativeY)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Assign all buttons to the respective directions.
        m_positiveX = positiveX;
        m_negativeX = negativeX;
        m_positiveY = positiveY;
        m_negativeY = negativeY;
        m_altPositiveX = altPositiveX;
        m_altNegativeX = altNegativeX;
        m_altPositiveY = altPositiveY;
        m_altNegativeY = altNegativeY;
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four gamepad buttons for either it's primary or secondary positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, GamepadControl positiveX, GamepadControl negativeX, GamepadControl positiveY, GamepadControl negativeY, GamepadControl altPositiveX, GamepadControl altNegativeX, GamepadControl altPositiveY, GamepadControl altNegativeY)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Assign all buttons to the respective directions.
        m_positiveX = Input.GamepadControlToKeyCode(positiveX);
        m_negativeX = Input.GamepadControlToKeyCode(negativeX);
        m_positiveY = Input.GamepadControlToKeyCode(positiveY);
        m_negativeY = Input.GamepadControlToKeyCode(negativeY);
        m_altPositiveX = Input.GamepadControlToKeyCode(altPositiveX);
        m_altNegativeX = Input.GamepadControlToKeyCode(altNegativeX);
        m_altPositiveY = Input.GamepadControlToKeyCode(altPositiveY);
        m_altNegativeY = Input.GamepadControlToKeyCode(altNegativeY);
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four keys for either it's primary or secondary positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, KeyCode positiveX, KeyCode negativeX, KeyCode positiveY, KeyCode negativeY, KeyCode altPositiveX, KeyCode altNegativeX, KeyCode altPositiveY, KeyCode altNegativeY, System.Action onRebind)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Bind onRebind callback.
        this.onRebind = onRebind;

        // Assign all buttons to the respective directions.
        m_positiveX = positiveX;
        m_negativeX = negativeX;
        m_positiveY = positiveY;
        m_negativeY = negativeY;
        m_altPositiveX = altPositiveX;
        m_altNegativeX = altNegativeX;
        m_altPositiveY = altPositiveY;
        m_altNegativeY = altNegativeY;
    }

    /// <summary>
    /// Create a Virtual Axis that reads from a set of four gamepad buttons for either it's primary or secondary positive X/Y and negative X/Y values. <br/>
    /// For Joystick axes, use the alterante constructor <see cref="ContextualAxis.ContextualAxis(string, AvailableInputAxes)"/> <br/>
    /// </summary>
    public ContextualAxis(string label, GamepadControl positiveX, GamepadControl negativeX, GamepadControl positiveY, GamepadControl negativeY, GamepadControl altPositiveX, GamepadControl altNegativeX, GamepadControl altPositiveY, GamepadControl altNegativeY, System.Action onRebind)
    {
        // Set the input source as a button axis.
        inputSource = AvailableInputAxes.ButtonAxis;

        // Set the label to the provided one, for navigation sake.
        this.label = label;

        // Bind onRebind callback.
        this.onRebind = onRebind;

        // Assign all buttons to the respective directions.
        m_positiveX = Input.GamepadControlToKeyCode(positiveX);
        m_negativeX = Input.GamepadControlToKeyCode(negativeX);
        m_positiveY = Input.GamepadControlToKeyCode(positiveY);
        m_negativeY = Input.GamepadControlToKeyCode(negativeY);
        m_altPositiveX = Input.GamepadControlToKeyCode(altPositiveX);
        m_altNegativeX = Input.GamepadControlToKeyCode(altNegativeX);
        m_altPositiveY = Input.GamepadControlToKeyCode(altPositiveY);
        m_altNegativeY = Input.GamepadControlToKeyCode(altNegativeY);
    }

    // Methods
    public override AbstractContextualAction Clone()
    {
        switch (inputSource)
        {
            case AvailableInputAxes.LeftStick or AvailableInputAxes.RightStick or AvailableInputAxes.MouseAxis: return new ContextualAxis(label, inputSource, onRebind);
            case AvailableInputAxes.ButtonAxis:
                return new ContextualAxis(label, m_positiveX, m_negativeX, m_positiveY, m_negativeY, m_altPositiveX, m_altNegativeX, m_altPositiveY, m_altNegativeY, onRebind);
            default: return null;
        }
    }
}

/// <summary>
/// A class that contains a list of virtual inputs for player actions on a specific platform.
/// </summary>
[System.Serializable]
public class PlatformInputProfile : InputProfile
{
    /// <summary>
    /// The platform this InputProfile is created for.
    /// </summary>
    public Input.InputIconDisplayType platform { get; protected set; }

    // Constructors

    /// <summary>
    /// Create an input profile for a target platform.
    /// </summary>
    /// <param name="targetPlatform"></param>
    /// <param name="actions"></param>
    public PlatformInputProfile(Input.InputIconDisplayType targetPlatform, params AbstractContextualAction[] actions)
    {
        platform = targetPlatform;
        this.actions = actions;
    }

    // Methods
    /// <summary>
    /// Duplicates all underlying ContextualActions and sets the Platform of the clone to that of the original.
    /// </summary>
    /// <returns><see cref="InputProfile"/></returns>
    public override InputProfile Clone()
    {
        AbstractContextualAction[] duplicateActions = new AbstractContextualAction[actions.Length];

        for (int i = 0; i < duplicateActions.Length; i++)
        {
            duplicateActions[i] = actions[i].Clone();
        }

        return new PlatformInputProfile(platform, duplicateActions);
    }
}

#endif