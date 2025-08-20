using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Irihi.Avalonia.Shared.Helpers;

namespace Ursa.Controls;

public class NumPad: TemplatedControl
{
    public static readonly StyledProperty<InputElement?> TargetProperty = AvaloniaProperty.Register<NumPad, InputElement?>(
        nameof(Target));

    public InputElement? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Target Ŀ���ڲ��� TextBox �� TextPresenter �ؼ� 
    /// </summary>
    private TextBox? _targetInnerText;

    public static readonly StyledProperty<bool> NumModeProperty = AvaloniaProperty.Register<NumPad, bool>(
        nameof(NumMode), defaultValue: true);

    public bool NumMode
    {
        get => GetValue(NumModeProperty);
        set => SetValue(NumModeProperty, value);
    }

    public static readonly AttachedProperty<bool> AttachProperty =
        AvaloniaProperty.RegisterAttached<NumPad, InputElement, bool>("Attach");

    public static void SetAttach(InputElement obj, bool value) => obj.SetValue(AttachProperty, value);
    public static bool GetAttach(InputElement obj) => obj.GetValue(AttachProperty);

    static NumPad()
    {
        AttachProperty.Changed.AddClassHandler<InputElement, bool>(OnAttachNumPad);
    }

    private static void OnAttachNumPad(InputElement input, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.NewValue.Value)
        {
            GotFocusEvent.AddHandler(OnTargetGotFocus, input);
        }
        else
        {
            GotFocusEvent.RemoveHandler(OnTargetGotFocus, input);
        }
    }
    
    private static void OnTargetGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not InputElement) return;
        var existing = OverlayDialog.Recall<NumPad>(null);
        if (existing is not null)
        {
            if(existing.Target is IPv4Box pv4Box)
            {
                pv4Box.IsTargetByNumPad = false; // ȡ�� IPv4Box �� NumPad ����ģʽ
            }
            existing.Target = sender as InputElement;
            existing._targetInnerText = FindTextBoxInTarget((sender as InputElement)!);

            if (existing.Target is IPv4Box pv4Box2)
            {
                pv4Box2.IsTargetByNumPad = true;
            }
            return;
        }
        var numPad = new NumPad() 
        {
            Target = sender as InputElement ,
            _targetInnerText = FindTextBoxInTarget((sender as InputElement)!)
        };
        OverlayDialog.Show(numPad, new object(), options: new OverlayDialogOptions() { Buttons = DialogButton.None });
    }

    private static readonly Dictionary<Key, string> KeyInputMapping = new()
    {
        [Key.NumPad0] = "0",
        [Key.NumPad1] = "1",
        [Key.NumPad2] = "2",
        [Key.NumPad3] = "3",
        [Key.NumPad4] = "4",
        [Key.NumPad5] = "5",
        [Key.NumPad6] = "6",
        [Key.NumPad7] = "7",
        [Key.NumPad8] = "8",
        [Key.NumPad9] = "9",
        [Key.Add] = "+",
        [Key.Subtract] = "-",
        [Key.Multiply] = "*",
        [Key.Divide] = "/",
        [Key.Decimal] = ".",
    };

    public void ProcessClick(object o)
    {
        if (Target is null || o is not NumPadButton b) return;
        var key = (b.NumMode ? b.NumKey : b.FunctionKey)?? Key.None;

        // ��������ڲ�Ϊ TextBox ��Ŀ��ؼ�����ʹ�ø� TextBox ��Ϊ����Ŀ��
        var realTarget = _targetInnerText ?? Target;
        if (KeyInputMapping.TryGetValue(key, out var s))
        {
            realTarget.RaiseEvent(new TextInputEventArgs()
            {
                Source = this,
                RoutedEvent = TextInputEvent,
                Text = s,
            });
        }
        else
        {
            realTarget.RaiseEvent(new KeyEventArgs()
            {
                Source = this,
                RoutedEvent = KeyDownEvent,
                Key = key,
            });
        }
    }
    /// <summary>
    /// ��Ŀ��ؼ��в��� TextBox �ؼ�
    /// </summary>
    /// <param name="target">Ŀ��ؼ�</param>
    /// <returns>�ҵ��� TextBox�����û���ҵ��򷵻� null</returns>
    private static TextBox? FindTextBoxInTarget(InputElement target)
    {
        // ���Ŀ�걾����� TextBox
        if (target is TextBox textBox)
            return textBox;
        
        // ���Ŀ���� TemplatedControl�������Ѿ�Ӧ����ģ��
        if (target is TemplatedControl templatedControl && templatedControl.IsInitialized)
        {
            // ����ͨ��ģ����� PART_TextBox
            if (templatedControl.GetTemplateChildren().FirstOrDefault(c => c is TextBox) is TextBox partTextBox)
                return partTextBox;
        }

        // ���Ŀ���� ILogical��ʹ�� LogicalTree ��չ��������
        if (target is ILogical logical)
        {
            // ʹ�� GetLogicalDescendants �������������߼��ӿؼ�
            var textBoxes = logical.GetLogicalDescendants().OfType<TextBox>();
            return textBoxes.FirstOrDefault();
        }

        return null;
    }
}