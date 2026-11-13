using Godot;
using System;
using System.Collections.Generic;

public partial class MainUI : Control
{
    private readonly Dictionary<string, LocationData> _locations = new();
    private string _currentLocation = "dormitory";
    private string _selectedEntity = "player";
    private string _activeTab = "Explore";
    private bool _hygieneDone;
    private bool _questAccepted;
    private bool _relicCollected;
    private bool _questReported;
    private int _day = 1;
    private int _hour = 7;
    private int _minute = 30;
    private int _playerHp = 20;
    private int _slimeHp = 12;

    private VBoxContainer _sidebar;
    private VBoxContainer _mainContent;
    private VBoxContainer _contextContent;
    private Label _locationTitle;
    private Label _clockLabel;
    private Label _feedbackLabel;
    private PanelContainer _contextPanel;

    private readonly Color _background = new("#171a20");
    private readonly Color _surface = new("#222731");
    private readonly Color _surfaceAlt = new("#2b323e");
    private readonly Color _accent = new("#c6a56b");
    private readonly Color _text = new("#e8e2d6");
    private readonly Color _muted = new("#9ba3ad");

    public override void _Ready()
    {
        BuildLocations();
        BuildInterface();
        ShowExplore();
        SelectEntity("player");
    }

    private void BuildLocations()
    {
        _locations["dormitory"] = new LocationData("Dormitory", "Your room at the academy. A quiet place to prepare for the day.", new[] { "Courtyard" }, new[] { "Yourself", "Bed", "Washbasin" });
        _locations["courtyard"] = new LocationData("Courtyard", "Students gather beneath the old trees before morning lessons.", new[] { "Dormitory", "Classroom", "Guild Hall" }, new[] { "Mira", "Training Dummy" });
        _locations["classroom"] = new LocationData("Classroom", "A practical classroom filled with maps, field notes and training equipment.", new[] { "Courtyard" }, new[] { "Instructor", "Notice Board" });
        _locations["guild_hall"] = new LocationData("Guild Hall", "The academy's expedition desk. Reports and requests are handled here.", new[] { "Courtyard", "Dungeon Entrance" }, new[] { "Guild Clerk", "Quest Board" });
        _locations["dungeon_entrance"] = new LocationData("Dungeon Entrance", "A supervised training site just beyond the academy grounds.", new[] { "Guild Hall", "Monster Room" }, new[] { "Dungeon Gate" });
        _locations["monster_room"] = new LocationData("Monster Room", "A small chamber where a training slime waits for new adventurers.", new[] { "Dungeon Entrance", "Relic Chamber" }, new[] { "Training Slime" });
        _locations["relic_chamber"] = new LocationData("Relic Chamber", "A sealed chamber containing the survey relic.", new[] { "Monster Room" }, new[] { "Survey Relic" });
    }

    private void BuildInterface()
    {
        var root = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        root.AddThemeConstantOverride("separation", 0);
        AddChild(root);

        var sidebarPanel = MakePanel(_surface, 0);
        sidebarPanel.CustomMinimumSize = new Vector2(210, 0);
        root.AddChild(sidebarPanel);
        _sidebar = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _sidebar.AddThemeConstantOverride("separation", 8);
        sidebarPanel.AddChild(_sidebar);
        BuildSidebar();

        var center = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        center.AddThemeConstantOverride("separation", 12);
        center.AddThemeConstantOverride("margin_left", 22);
        center.AddThemeConstantOverride("margin_right", 22);
        center.AddThemeConstantOverride("margin_top", 18);
        center.AddThemeConstantOverride("margin_bottom", 18);
        root.AddChild(center);

        var header = new HBoxContainer();
        center.AddChild(header);
        var heading = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        header.AddChild(heading);
        _locationTitle = MakeLabel("Adventurer Academy", 25, _text, true);
        heading.AddChild(_locationTitle);
        _clockLabel = MakeLabel("DAY 01  ·  07:30", 12, _muted);
        heading.AddChild(_clockLabel);
        var status = MakeLabel("HP  " + _playerHp + " / 20", 13, _accent, true);
        header.AddChild(status);

        var rule = new HSeparator();
        center.AddChild(rule);
        _mainContent = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _mainContent.AddThemeConstantOverride("separation", 12);
        center.AddChild(_mainContent);

        _contextPanel = MakePanel(_surface, 0);
        _contextPanel.CustomMinimumSize = new Vector2(260, 0);
        root.AddChild(_contextPanel);
        _contextContent = new VBoxContainer();
        _contextContent.AddThemeConstantOverride("separation", 10);
        _contextPanel.AddChild(_contextContent);
        _feedbackLabel = MakeLabel("Select something in the scene to inspect it.", 12, _muted);
        _feedbackLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _contextContent.AddChild(MakeLabel("INTERACTION", 11, _accent, true));
        _contextContent.AddChild(_feedbackLabel);
    }

    private void BuildSidebar()
    {
        _sidebar.AddChild(MakeLabel("ASTERIA", 20, _accent, true));
        _sidebar.AddChild(MakeLabel("ADVENTURER ACADEMY", 10, _muted));
        _sidebar.AddChild(new HSeparator());
        AddSidebarGroup("WORLD", new[] { "Explore", "Map" });
        AddSidebarGroup("CHARACTER", new[] { "Profile", "Relationships", "Inventory" });
        AddSidebarGroup("ADVENTURE", new[] { "Quests", "Skills" });
        AddSidebarGroup("SYSTEM", new[] { "Settings" });
        _sidebar.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        _sidebar.AddChild(MakeLabel("FIRST ADVENTURE  ·  PROTOTYPE", 9, _muted));
    }

    private void AddSidebarGroup(string title, string[] tabs)
    {
        _sidebar.AddChild(MakeLabel(title, 10, _accent, true));
        foreach (string tab in tabs)
        {
            var button = new Button { Text = tab, Alignment = HorizontalAlignment.Left, CustomMinimumSize = new Vector2(0, 36), FocusMode = FocusModeEnum.None };
            button.AddThemeFontSizeOverride("font_size", 13);
            button.AddThemeColorOverride("font_color", _text);
            button.AddThemeStyleboxOverride("normal", FlatStyle(_surface, 6, 8));
            button.AddThemeStyleboxOverride("hover", FlatStyle(_surfaceAlt, 6, 8));
            button.AddThemeStyleboxOverride("pressed", FlatStyle(new Color("#3a414c"), 6, 8));
            button.Pressed += () => OpenTab(tab);
            _sidebar.AddChild(button);
        }
    }

    private void OpenTab(string tab)
    {
        _activeTab = tab;
        ClearChildren(_mainContent);
        _locationTitle.Text = tab == "Explore" ? _locations[_currentLocation].Name : tab;
        if (tab == "Explore") ShowExplore();
        else if (tab == "Map") ShowMap();
        else ShowSimpleTab(tab);
    }

    private void ShowExplore()
    {
        _activeTab = "Explore";
        ClearChildren(_mainContent);
        LocationData location = _locations[_currentLocation];
        _locationTitle.Text = location.Name;
        _clockLabel.Text = "DAY " + _day.ToString("00") + "  ·  " + _hour.ToString("00") + ":" + _minute.ToString("00") + "  ·  ACADEMY GROUNDS";
        AddSection("SCENE", location.Description);
        AddSection("PEOPLE", "People currently present");
        foreach (string person in location.People) AddEntityButton(person, "NPC", person == "Instructor" ? "Available for briefing" : "Going about their day");
        AddSection("OBJECTS", "Things you can inspect or use");
        foreach (string item in location.Objects) AddEntityButton(item, "OBJECT", GetObjectState(item));
        AddSection("EXITS", "Connected locations");
        foreach (string exit in location.Exits) AddLocationButton(exit);
    }

    private void ShowMap()
    {
        _activeTab = "Map";
        ClearChildren(_mainContent);
        _locationTitle.Text = "Academy & Training Grounds";
        AddSection("WORLD MAP", "Choose a connected location to travel. Only adjacent locations are available.");
        foreach (KeyValuePair<string, LocationData> pair in _locations)
        {
            string state = pair.Key == _currentLocation ? "CURRENT LOCATION" : "LOCATION";
            AddLocationButton(pair.Value.Name, pair.Key, state);
        }
    }

    private void ShowSimpleTab(string tab)
    {
        _locationTitle.Text = tab;
        string description = tab switch
        {
            "Profile" => "Your adventurer profile and current condition.",
            "Relationships" => "People you have met at the academy.",
            "Inventory" => "Items collected during your journey.",
            "Quests" => "Your current assignment and its objectives.",
            "Skills" => "Combat techniques available to a novice adventurer.",
            _ => "Prototype settings."
        };
        AddSection(tab.ToUpper(), description);
        if (tab == "Profile")
        {
            AddInfoRow("Health", _playerHp + " / 20");
            AddInfoRow("Day", _day.ToString());
            AddInfoRow("Hygiene", _hygieneDone ? "Completed" : "Not completed");
        }
        else if (tab == "Quests")
        {
            AddInfoRow("First Dungeon Survey", _questReported ? "Completed" : _questAccepted ? "In progress" : "Not accepted");
            AddInfoRow("Briefing", _questAccepted ? "Completed" : "Not started");
            AddInfoRow("Training Slime", _slimeHp <= 0 ? "Defeated" : "Not defeated");
            AddInfoRow("Survey Relic", _relicCollected ? "Collected" : "Not collected");
            AddInfoRow("Report", _questReported ? "Submitted" : "Pending");
        }
        else if (tab == "Inventory") AddInfoRow("Survey Relic", _relicCollected ? "1" : "0");
        else if (tab == "Relationships") AddInfoRow("Instructor", _questAccepted ? "Briefing attended" : "Not yet acquainted");
        else if (tab == "Skills") AddInfoRow("Basic Attack", "5 damage");
        else AddInfoRow("Build", "Godot 4 .NET · UI prototype");
    }

    private void AddSection(string title, string subtitle)
    {
        _mainContent.AddChild(MakeLabel(title, 11, _accent, true));
        var label = MakeLabel(subtitle, 14, _text);
        label.AutowrapMode = TextServer.AutowrapMode.Word;
        _mainContent.AddChild(label);
    }

    private void AddEntityButton(string name, string type, string state)
    {
        var button = new EntityButton { Text = name + "   ·   " + state, EntityId = name, CustomMinimumSize = new Vector2(0, 40), Alignment = HorizontalAlignment.Left, FocusMode = FocusModeEnum.None };
        button.EntityPressed += OnEntitySelected;
        StyleButton(button);
        _mainContent.AddChild(button);
    }

    private void AddLocationButton(string name)
    {
        string id = FindLocationId(name);
        AddLocationButton(name, id, "TRAVEL");
    }

    private void AddLocationButton(string name, string id, string state)
    {
        var button = new EntityButton { Text = name + "   ·   " + state, EntityId = id, CustomMinimumSize = new Vector2(0, 38), Alignment = HorizontalAlignment.Left, FocusMode = FocusModeEnum.None };
        button.EntityPressed += OnLocationSelected;
        StyleButton(button);
        _mainContent.AddChild(button);
    }

    private void OnLocationSelected(string id)
    {
        if (!_locations.ContainsKey(id)) return;
        if (id != _currentLocation && !_locations[_currentLocation].Exits.Contains(_locations[id].Name))
        {
            SetFeedback("That location is not connected to your current position.");
            return;
        }
        _currentLocation = id;
        _selectedEntity = "";
        ShowExplore();
        SetFeedback("You travelled to " + _locations[id].Name + ".");
        SelectEntity("Yourself");
    }

    private void OnEntitySelected(string id)
    {
        SelectEntity(id);
    }

    private void SelectEntity(string id)
    {
        _selectedEntity = id;
        ClearChildren(_contextContent);
        _contextContent.AddChild(MakeLabel("INTERACTION", 11, _accent, true));
        _contextContent.AddChild(MakeLabel(id, 20, _text, true));
        string type = GetEntityType(id);
        _contextContent.AddChild(MakeLabel(type, 10, _muted));
        var description = MakeLabel(GetEntityDescription(id), 13, _text);
        description.AutowrapMode = TextServer.AutowrapMode.Word;
        _contextContent.AddChild(description);
        _contextContent.AddChild(new HSeparator());
        _contextContent.AddChild(MakeLabel("STATE", 10, _accent, true));
        _contextContent.AddChild(MakeLabel(GetObjectState(id), 12, _muted));
        _contextContent.AddChild(MakeLabel("ACTIONS", 10, _accent, true));
        foreach (string action in GetActions(id))
        {
            var button = new Button { Text = action, CustomMinimumSize = new Vector2(0, 38), FocusMode = FocusModeEnum.None };
            StyleButton(button);
            button.Pressed += () => ExecuteAction(id, action);
            _contextContent.AddChild(button);
        }
    }

    private void ExecuteAction(string entity, string action)
    {
        if (action == "Talk")
        {
            if (entity == "Instructor" && _currentLocation == "classroom") { _questAccepted = true; SetFeedback("Instructor: Your first assignment is to survey the training dungeon."); }
            else SetFeedback(entity + ": There is not much to say right now.");
        }
        else if (action == "Accept Quest") { _questAccepted = true; SetFeedback("Quest accepted: First Dungeon Survey."); }
        else if (action == "Brush Teeth") { _hygieneDone = true; AdvanceTime(10); SetFeedback("You freshen up. Hygiene objective completed."); }
        else if (action == "Sleep")
        {
            if (_currentLocation != "dormitory") SetFeedback("You can only sleep in the Dormitory.");
            else if (!_questReported || !_hygieneDone) SetFeedback("Before sleeping: report the quest and complete your hygiene routine.");
            else { _day++; _hour = 7; _minute = 0; _hygieneDone = false; SetFeedback("A new day begins. Your completed quest remains recorded."); }
        }
        else if (action == "Attack")
        {
            if (_slimeHp <= 0) SetFeedback("The Training Slime has already been defeated.");
            else
            {
                _slimeHp = Math.Max(0, _slimeHp - 5);
                if (_slimeHp > 0) _playerHp = Math.Max(0, _playerHp - 4);
                SetFeedback(_slimeHp == 0 ? "Victory! The Training Slime is defeated." : "You deal 5 damage. The slime retaliates for 4 damage.");
            }
        }
        else if (action == "Defend") { SetFeedback("You take a defensive stance. Incoming damage is reduced."); }
        else if (action == "Collect") { _relicCollected = true; SetFeedback("Survey Relic collected. Return to the Guild Hall to report."); }
        else if (action == "Report") { if (_relicCollected) { _questReported = true; SetFeedback("Quest completed. Reward: 100 EXP and 20 Gold."); } else SetFeedback("You still need to collect the Survey Relic."); }
        else if (action == "Inspect") SetFeedback(GetEntityDescription(entity));
        else if (action == "Rest") { _playerHp = 20; AdvanceTime(30); SetFeedback("You take a break and recover your strength."); }
        else if (action == "Gift") SetFeedback("You do not have an appropriate gift yet.");
        else if (action == "Open") SetFeedback("The way is clear.");
        else if (action == "Enter") { SetFeedback("Choose the Dungeon Entrance from the map exits."); }
        else if (action == "Wash Face") { _hygieneDone = true; AdvanceTime(5); SetFeedback("You wash your face."); }
        else if (action == "Change Outfit") SetFeedback("You change into your academy uniform.");
        else if (action == "Invite") SetFeedback("This character is not available to invite.");
        RefreshCurrentView();
    }

    private void RefreshCurrentView()
    {
        if (_activeTab == "Explore") ShowExplore();
        else if (_activeTab == "Map") ShowMap();
        else ShowSimpleTab(_activeTab);
        SelectEntity(_selectedEntity);
    }

    private string[] GetActions(string id)
    {
        if (id == "Yourself" || id == "player") return new[] { "Inspect", "Rest", "Brush Teeth", "Wash Face", "Change Outfit" };
        if (id == "Bed") return new[] { "Sleep", "Rest", "Inspect" };
        if (id == "Washbasin") return new[] { "Brush Teeth", "Wash Face", "Inspect" };
        if (id == "Instructor") return new[] { "Talk", "Inspect" };
        if (id == "Mira" || id == "Guild Clerk") return new[] { "Talk", "Gift", "Inspect", "Invite" };
        if (id == "Training Slime") return new[] { "Inspect", "Attack", "Defend" };
        if (id == "Survey Relic") return new[] { "Collect", "Inspect" };
        if (id == "Quest Board") return new[] { "Accept Quest", "Inspect" };
        if (id == "Dungeon Gate") return new[] { "Enter", "Inspect" };
        return new[] { "Inspect" };
    }

    private string GetEntityType(string id)
    {
        if (id == "Yourself" || id == "player") return "PLAYER CHARACTER";
        if (id == "Bed" || id == "Washbasin" || id == "Training Dummy" || id == "Survey Relic" || id == "Quest Board" || id == "Dungeon Gate") return "OBJECT";
        return "NPC";
    }

    private string GetEntityDescription(string id)
    {
        return id switch
        {
            "Yourself" or "player" => "A first-year student of the Adventurer Academy.",
            "Instructor" => "A senior adventurer responsible for practical training.",
            "Mira" => "A fellow student who spends most mornings in the courtyard.",
            "Bed" => "Your bed. A good night's rest starts here.",
            "Washbasin" => "A washbasin for your morning routine.",
            "Training Slime" => "A supervised training monster. It is hostile during encounters.",
            "Survey Relic" => "A small relic used to mark and verify dungeon survey routes.",
            _ => "An object or character in this location."
        };
    }

    private string GetObjectState(string id)
    {
        if (id == "Yourself" || id == "player") return "HP " + _playerHp + " / 20";
        if (id == "Training Slime") return _slimeHp <= 0 ? "Defeated" : "HP " + _slimeHp + " / 12";
        if (id == "Survey Relic") return _relicCollected ? "Collected" : "Unclaimed";
        if (id == "Bed") return "Available";
        if (id == "Washbasin") return _hygieneDone ? "Routine completed" : "Ready to use";
        return "Available";
    }

    private string FindLocationId(string name)
    {
        foreach (KeyValuePair<string, LocationData> pair in _locations) if (pair.Value.Name == name) return pair.Key;
        return "";
    }

    private void AddInfoRow(string title, string value)
    {
        var row = new HBoxContainer();
        row.AddChild(MakeLabel(title, 13, _muted));
        var valueLabel = MakeLabel(value, 13, _text, true);
        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        valueLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(valueLabel);
        _mainContent.AddChild(row);
    }

    private void AdvanceTime(int minutes)
    {
        int total = _hour * 60 + _minute + minutes;
        _hour = (total / 60) % 24;
        _minute = total % 60;
    }

    private void SetFeedback(string message)
    {
        _feedbackLabel.Text = message;
    }

    private PanelContainer MakePanel(Color color, int padding)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", FlatStyle(color, 0, padding));
        return panel;
    }

    private StyleBoxFlat FlatStyle(Color color, int radius, int padding)
    {
        var style = new StyleBoxFlat { BgColor = color, CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius, CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius };
        style.ContentMarginLeft = padding;
        style.ContentMarginRight = padding;
        style.ContentMarginTop = padding;
        style.ContentMarginBottom = padding;
        return style;
    }

    private Label MakeLabel(string value, int size, Color color, bool bold = false)
    {
        var label = new Label { Text = value };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        if (bold) label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0));
        return label;
    }

    private void StyleButton(Button button)
    {
        button.AddThemeFontSizeOverride("font_size", 12);
        button.AddThemeColorOverride("font_color", _text);
        button.AddThemeStyleboxOverride("normal", FlatStyle(_surfaceAlt, 5, 9));
        button.AddThemeStyleboxOverride("hover", FlatStyle(new Color("#3a414c"), 5, 9));
        button.AddThemeStyleboxOverride("pressed", FlatStyle(new Color("#4b4d4d"), 5, 9));
    }

    private void ClearChildren(Node node)
    {
        foreach (Node child in node.GetChildren()) child.QueueFree();
    }

    private sealed class LocationData
    {
        public string Name { get; }
        public string Description { get; }
        public string[] Exits { get; }
        public string[] People { get; }
        public string[] Objects { get; }

        public LocationData(string name, string description, string[] exits, string[] contents)
        {
            Name = name;
            Description = description;
            Exits = exits;
            var people = new List<string>();
            var objects = new List<string>();
            foreach (string item in contents)
            {
                if (item == "Yourself" || item == "Bed" || item == "Washbasin" || item == "Training Dummy" || item == "Notice Board" || item == "Quest Board" || item == "Dungeon Gate" || item == "Survey Relic" || item == "Training Slime") objects.Add(item);
                else people.Add(item);
            }
            People = people.ToArray();
            Objects = objects.ToArray();
        }
    }

    private sealed class EntityButton : Button
    {
        public string EntityId { get; set; }
        public event Action<string> EntityPressed;

        public EntityButton()
        {
            Pressed += HandlePressed;
        }

        private void HandlePressed()
        {
            EntityPressed?.Invoke(EntityId);
        }
    }
}
