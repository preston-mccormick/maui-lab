using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;

namespace MauiLab.Contacts;

public partial class ContactCard : Border
{
    const string Placeholder = "contact_placeholder.png";

    ContactItemModel? _item;
    View? _form;
    Entry? _focus;
    int _generation;
    bool _busy;

    public ContactCard()
    {
        InitializeComponent();
        InfoBlock.SizeChanged += (_, _) => MatchPhotoToInfo();
    }

    void MatchPhotoToInfo()
    {
        if (InfoBlock.Height <= 0)
            return;

        var height = Math.Max(148, InfoBlock.Height);
        if (Math.Abs(PhotoFrame.HeightRequest - height) <= 0.5)
            return;

        PhotoFrame.HeightRequest = height;
        PhotoImage.HeightRequest = height;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_item != null)
            _item.PropertyChanged -= OnItemPropertyChanged;

        _generation++;
        _busy = false;
        _item = BindingContext as ContactItemModel;
        if (_item == null)
            return;

        _item.PropertyChanged += OnItemPropertyChanged;
        Snap(_item);
    }

    void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_item == null)
            return;

        if (e.PropertyName == nameof(ContactItemModel.IsEditing))
        {
            if (_item.IsEditing)
                _ = OpenAsync(_item);
            else
                _ = CloseAsync();
            return;
        }

        if (e.PropertyName is nameof(ContactItemModel.PhotoPath) or nameof(ContactItemModel.PhotoFile))
            UpdatePhoto(_item);
    }

    void Snap(ContactItemModel item)
    {
        _generation++;
        _busy = false;
        InputTransparent = false;
        SetChrome(true);
        HeightRequest = -1;
        RemoveForm();
        ContactFlip.Flatten(ReadFace);
        ReadFace.IsVisible = !item.IsEditing;
        if (item.IsEditing)
        {
            var form = BuildForm(item);
            ContactFlip.Flatten(form);
            FaceHost.Children.Add(form);
            _form = form;
            ApplyShadow(open: true);
        }
        else
            ApplyShadow(open: false);

        UpdatePhoto(item);
    }

    async Task OpenAsync(ContactItemModel item)
    {
        if (_busy || _form != null)
            return;

        var generation = ++_generation;
        _busy = true;
        InputTransparent = true;
        SetChrome(false);
        var form = BuildForm(item);
        form.IsVisible = false;
        FaceHost.Children.Add(form);
        await Dispatcher.DispatchAsync(() => { });
        if (Aborted(generation, form))
            return;

        ReadFace.IsVisible = true;
        await ContactFlip.TurnOut(ReadFace);
        if (Aborted(generation, form))
            return;

        ReadFace.IsVisible = false;
        form.IsVisible = true;
        ContactFlip.Park(form);
        await ContactFlip.TurnIn(form);
        if (Aborted(generation, form))
            return;

        _form = form;
        _busy = false;
        InputTransparent = false;
        SetChrome(true);
        ApplyShadow(open: true);
        if (item.Contact.Id == 0)
            _focus?.Focus();
    }

    bool Aborted(int generation, View form)
    {
        if (generation == _generation && _item?.IsEditing == true)
            return false;

        FaceHost.Children.Remove(form);
        return true;
    }

    async Task CloseAsync()
    {
        var generation = ++_generation;
        var form = _form;
        var item = _item;
        if (form == null)
        {
            _busy = false;
            InputTransparent = false;
            SetChrome(true);
            ReadFace.IsVisible = true;
            ContactFlip.Flatten(ReadFace);
            ApplyShadow(open: false);
            return;
        }
        _busy = true;
        InputTransparent = true;
        SetChrome(false);
        await ContactFlip.TurnOut(form);
        if (generation != _generation)
            return;

        RemoveForm();
        ReadFace.IsVisible = true;
        ContactFlip.Park(ReadFace);
        await ContactFlip.TurnIn(ReadFace);
        if (generation != _generation)
            return;

        _busy = false;
        InputTransparent = false;
        SetChrome(true);
        ApplyShadow(open: false);
        if (item != null)
            Page?.Reveal(item);
    }

    void RemoveForm()
    {
        if (_form == null)
            return;

        FaceHost.Children.Remove(_form);
        _form = null;
        _focus = null;
    }

    void ApplyShadow(bool open)
    {
        Shadow = new Shadow
        {
            Brush = Brush.Black,
            Offset = open ? new Point(0, 10) : new Point(0, 2),
            Radius = open ? 18 : 6,
            Opacity = open ? 0.32f : 0.18f
        };
        CardShape.CornerRadius = open ? 18 : 12;
    }

    void UpdatePhoto(ContactItemModel item)
    {
        PhotoImage.Source = PhotoSource(item.PhotoPath);
    }

    static ImageSource PhotoSource(string path) =>
        path.Length > 0 && File.Exists(path) ? ImageSource.FromFile(path) : ImageSource.FromFile(Placeholder);

    async void OnEdit(object? sender, EventArgs e)
    {
        if (_busy || _item == null || PageModel is not { } model)
            return;

        await model.EditAsync(_item);
    }

    View BuildForm(ContactItemModel item)
    {
        _focus = null;
        var photo = new Image
        {
            Source = PhotoSource(item.PhotoPath),
            Aspect = Aspect.AspectFill,
            HeightRequest = 180,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ContactItemModel.PhotoPath) or nameof(ContactItemModel.PhotoFile))
                photo.Source = PhotoSource(item.PhotoPath);
        };
        var photoFrame = new Border
        {
            Padding = 0,
            StrokeThickness = 0,
            HeightRequest = 180,
            HorizontalOptions = LayoutOptions.Fill,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = photo,
        };
        photoFrame.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#E5E5EA"), Color.FromArgb("#1C1C1E"));

        var change = ActionButton("Change photo");
        change.HorizontalOptions = LayoutOptions.Start;
        change.Clicked += async (_, _) => await ChoosePhotoAsync(item);

        var save = ActionButton("Save");
        save.HorizontalOptions = LayoutOptions.End;
        save.IsEnabled = item.CanSave;
        save.Clicked += (_, _) =>
        {
            if (!_busy)
                PageModel?.Save();
        };

        var fields = new VerticalStackLayout { Spacing = 10 };
        fields.Children.Add(new VerticalStackLayout
        {
            Spacing = 6,
            Children = { Caption("Photo"), photoFrame, change },
        });
        AddField(fields, item, save, "First name", nameof(ContactItemModel.FirstName), () => item.FirstName, value => item.FirstName = value);
        AddField(fields, item, save, "Last name", nameof(ContactItemModel.LastName), () => item.LastName, value => item.LastName = value);
        AddField(fields, item, save, "Street", nameof(ContactItemModel.Street), () => item.Street, value => item.Street = value);
        AddField(fields, item, save, "City", nameof(ContactItemModel.City), () => item.City, value => item.City = value);
        fields.Children.Add(StateField(item, save));
        AddField(fields, item, save, "Postal code", nameof(ContactItemModel.PostalCode), () => item.PostalCode, value => item.PostalCode = value);
        fields.Children.Add(PhoneField(item, save));
        AddField(fields, item, save, "Email", nameof(ContactItemModel.Email), () => item.Email, value => item.Email = value);
        fields.Children.Add(NotesField(item, save));

        var delete = ActionButton("Delete");
        delete.TextColor = Color.FromArgb("#FF3B30");
        delete.Clicked += async (_, _) =>
        {
            if (!_busy && PageModel is { } model)
                await model.DeleteAsync();
        };
        var cancel = ActionButton("Cancel");
        cancel.Clicked += (_, _) =>
        {
            if (!_busy)
                PageModel?.Cancel();
        };

        var buttons = new Grid
        {
            Margin = new Thickness(0, 4, 0, 0),
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        var left = new HorizontalStackLayout { Spacing = 8, Children = { delete, cancel } };
        buttons.Add(left);
        buttons.Add(save);
        Grid.SetColumn(save, 1);
        fields.Children.Add(buttons);
        return fields;
    }

    static Button ActionButton(string text) => new()
    {
        Text = text,
        FontSize = 17,
        MinimumHeightRequest = 48,
        Padding = new Thickness(16, 10),
    };

    static Label Caption(string text) => new()
    {
        Text = text,
        FontSize = 15,
        VerticalOptions = LayoutOptions.Center,
    };

    void AddField(VerticalStackLayout fields, ContactItemModel item, Button save, string caption, string property, Func<string> get, Action<string> set)
    {
        var entry = new Entry { Text = get(), HorizontalTextAlignment = TextAlignment.End };
        _focus ??= entry;
        UseFieldFill(entry);
        var error = ErrorLabel(item, property);
        entry.TextChanged += (_, e) =>
        {
            set(e.NewTextValue ?? "");
            ShowError(error, item, property);
            save.IsEnabled = item.CanSave;
        };
        fields.Children.Add(FieldRow(caption, entry, error));
    }

    static View StateField(ContactItemModel item, Button save)
    {
        var picker = new Picker { Title = "State", HorizontalTextAlignment = TextAlignment.End };
        UseFieldFill(picker);
        picker.Items.Add("");
        foreach (var (name, _) in UsStates.All)
            picker.Items.Add(name);

        picker.SelectedIndex = StateIndex(item.State);
        var error = ErrorLabel(item, nameof(ContactItemModel.State));
        picker.SelectedIndexChanged += (_, _) =>
        {
            item.State = picker.SelectedIndex <= 0 ? "" : UsStates.All[picker.SelectedIndex - 1].Code;
            ShowError(error, item, nameof(ContactItemModel.State));
            save.IsEnabled = item.CanSave;
        };
        return FieldRow("State", picker, error);
    }

    static View PhoneField(ContactItemModel item, Button save)
    {
        var entry = new Entry { Text = item.Phone, Keyboard = Keyboard.Telephone, HorizontalTextAlignment = TextAlignment.End };
        UseFieldFill(entry);
        entry.Behaviors.Add(new MaskedBehavior { Mask = "(XXX) XXX-XXXX" });
        var error = ErrorLabel(item, nameof(ContactItemModel.Phone));
        entry.TextChanged += (_, e) =>
        {
            var digits = ContactItemModel.Digits(e.NewTextValue ?? "");
            if (digits == item.Contact.Phone)
                return;

            item.Phone = e.NewTextValue ?? "";
            ShowError(error, item, nameof(ContactItemModel.Phone));
            save.IsEnabled = item.CanSave;
        };
        return FieldRow("Phone", entry, error);
    }

    static View NotesField(ContactItemModel item, Button save)
    {
        var editor = new Editor
        {
            Text = item.Notes,
            HeightRequest = 120,
            AutoSize = EditorAutoSizeOption.Disabled,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.End,
        };
        UseFieldFill(editor);
        var error = ErrorLabel(item, nameof(ContactItemModel.Notes));
        editor.TextChanged += (_, e) =>
        {
            item.Notes = e.NewTextValue ?? "";
            ShowError(error, item, nameof(ContactItemModel.Notes));
            save.IsEnabled = item.CanSave;
        };
        var box = new Border
        {
            Padding = new Thickness(8, 4),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = editor,
        };
        box.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#C7C7CC"), Color.FromArgb("#636366"));
        box.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#FFFFFF"), Color.FromArgb("#2C2C2E"));
        return new VerticalStackLayout
        {
            Spacing = 4,
            Children = { Caption("Notes"), box, error },
        };
    }

    static void UseFieldFill(VisualElement field)
    {
        field.SetAppThemeColor(VisualElement.BackgroundColorProperty, Color.FromArgb("#FFFFFF"), Color.FromArgb("#2C2C2E"));
        field.HandlerChanged += (_, _) => PaintFieldSurface(field);
    }

    static void PaintFieldSurface(VisualElement field)
    {
#if IOS || MACCATALYST
        if (field.Handler?.PlatformView is not UIKit.UIView view)
            return;

        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        view.BackgroundColor = dark ? UIKit.UIColor.FromRGB(0x2C, 0x2C, 0x2E) : UIKit.UIColor.White;
        if (view is UIKit.UITextField text)
            text.TextAlignment = UIKit.UITextAlignment.Right;
        else if (view is UIKit.UITextView editor)
            editor.TextAlignment = UIKit.UITextAlignment.Right;
#endif
    }

    static int StateIndex(string code)
    {
        for (var i = 0; i < UsStates.All.Count; i++)
        {
            if (UsStates.All[i].Code == code)
                return i + 1;
        }

        return 0;
    }

    static Grid FieldRow(string caption, View input, Label error)
    {
        input.HorizontalOptions = LayoutOptions.Fill;
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(104), new ColumnDefinition(GridLength.Star) },
            RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
        };
        grid.Add(Caption(caption));
        grid.Add(input);
        Grid.SetColumn(input, 1);
        grid.Add(error);
        Grid.SetColumn(error, 1);
        Grid.SetRow(error, 1);
        return grid;
    }

    static Label ErrorLabel(ContactItemModel item, string property)
    {
        var label = new Label { FontSize = 12, TextColor = Color.FromArgb("#FF3B30") };
        ShowError(label, item, property);
        return label;
    }

    static void ShowError(Label label, ContactItemModel item, string property)
    {
        label.Text = string.Join(" ", item.GetErrors(property).Cast<string>());
        label.IsVisible = label.Text.Length > 0;
    }

    async Task ChoosePhotoAsync(ContactItemModel item)
    {
        if (_busy || Page is not { } page || PageModel is not { } model)
            return;

        var photos = model.PickerPhotos.ToList();
        var choice = new PhotoChoice();
        choice.Select(photos.FirstOrDefault(path => path == item.PhotoFile));
        var list = new CollectionView
        {
            HeightRequest = 210,
            SelectionMode = SelectionMode.None,
            ItemsSource = photos,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 4 },
            ItemTemplate = new DataTemplate(() => new PhotoTile(model.DirectoryPath, choice)),
        };
        list.BackgroundColor = Color.FromArgb("#00000000");

        var ok = ActionButton("OK");
        var cancel = ActionButton("Cancel");
        ok.Clicked += async (_, _) => await page.ClosePopupAsync(choice.Selected ?? "");
        cancel.Clicked += async (_, _) => await page.ClosePopupAsync("");

        var title = new Label { Text = "Choose a photo", FontAttributes = FontAttributes.Bold, FontSize = 18 };
        title.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#000000"), Color.FromArgb("#FFFFFF"));
        var light = Color.FromArgb("#F2F2F7");
        var dark = Color.FromArgb("#1C1C1E");
        var popup = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 12,
            WidthRequest = 420,
            Children =
            {
                title,
                list,
                new HorizontalStackLayout
                {
                    HorizontalOptions = LayoutOptions.End,
                    Spacing = 8,
                    Children = { cancel, ok }
                }
            }
        };
        popup.SetAppThemeColor(BackgroundColorProperty, light, dark);
        var shape = new RoundRectangle { CornerRadius = new CornerRadius(16), StrokeThickness = 0 };
        shape.SetAppTheme<Brush>(Shape.FillProperty, new SolidColorBrush(light), new SolidColorBrush(dark));

        var result = await page.ShowPopupAsync<string>(popup, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            Shape = shape,
        });
        if (!result.WasDismissedByTappingOutsideOfPopup && !string.IsNullOrEmpty(result.Result))
            item.ChoosePhoto(result.Result);
    }

    void SetChrome(bool enabled) => Page?.SetChromeEnabled(enabled);

    ContactsPage? Page => Find<ContactsPage>();

    ContactsPageModel? PageModel => Page?.BindingContext as ContactsPageModel;

    T? Find<T>() where T : Element
    {
        Element? current = this;
        while (current != null)
        {
            if (current is T match)
                return match;
            current = current.Parent;
        }

        return null;
    }

}
