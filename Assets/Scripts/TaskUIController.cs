using UnityEngine;
using UnityEngine.UIElements;

public class TaskUIController : MonoBehaviour
{
    [SerializeField] UIDocument document;
    VisualElement rootElement;
    Button acceptBTN;
    Button closeBTN;
    Button redBTN;
    TextField inputField;
    GameStepper stepper;
    Toggle checkbox;
    GameDropdown dropdown;
    GameSlider slider;
    SteppedGameSlider steppedSlider;

    void Start()
    {
        rootElement = document.rootVisualElement;
        acceptBTN = rootElement.Q<Button>(name: "GreenButton");
        closeBTN = rootElement.Q<VisualElement>(name: "RightTopCloseBtn").Q<Button>();
        closeBTN.clicked += CloseBTN_onClick;
        redBTN = rootElement.Q<VisualElement>(name: "RedCrossContainer").Q<Button>();
        redBTN.clicked += RedBTN_clicked;
        inputField = rootElement.Q<VisualElement>(name: "InputFieldContainer").Q<TextField>();
        inputField.RegisterCallback<InputEvent>(InputField_onInput);
        stepper = rootElement.Q<GameStepper>();
        stepper.OnValueChange += Stepper_OnValueChange;
        checkbox = rootElement.Q<VisualElement>(name: "CheckboxContainer").Q<Toggle>();
        checkbox.RegisterValueChangedCallback(Checkbox_onValueChange);
        dropdown = rootElement.Q<GameDropdown>();
        dropdown.OnValueChange += Dropdown_OnValueChange;
        slider = rootElement.Q<GameSlider>(name: "NormalSlider");
        slider.OnValueChange += Slider_OnValueChange;
        steppedSlider = rootElement.Q<SteppedGameSlider>();
        steppedSlider.OnSteppedValueChange += SteppedSlider_OnSteppedValueChange;
        acceptBTN.SetEnabled(false);
        acceptBTN.enabledSelf = false;
        Debug.Log("Init UI successful");
    }

    private void SteppedSlider_OnSteppedValueChange()
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void Slider_OnValueChange()
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void Dropdown_OnValueChange()
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void Checkbox_onValueChange(ChangeEvent<bool> e)
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void Stepper_OnValueChange()
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void InputField_onInput(InputEvent e)
    {
        acceptBTN.SetEnabled(true);
        acceptBTN.enabledSelf = true;
    }

    private void RedBTN_clicked()
    {
        acceptBTN.SetEnabled(false);
        acceptBTN.enabledSelf = false;
        Debug.Log("Red clicked! Disabling accept button");
    }

    private void CloseBTN_onClick()
    {
        acceptBTN.SetEnabled(false);
        acceptBTN.enabledSelf = false;
        Debug.Log("Close button clicked! Disabling accept button");
    }
}
