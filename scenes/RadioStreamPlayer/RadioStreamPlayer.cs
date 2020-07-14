using Godot;
using Godot.Collections;
using System;
using NAudio.Wave;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Tags;

public class RadioStreamPlayer : Godot.Node
{
    
    private String radioUrl = "";
    private String radioName = "";
    private int radioIndex = -1;
    private float volume = 0f;
    private MediaFoundationReader mediaFoundationReader;    
    private WaveOutEvent waveOutEvent = new WaveOutEvent();
    private StationHandler StationHandler;

    VBoxContainer PlayContainer;
    VBoxContainer StopContainer;
    TextureButton PlayBtn;
    TextureButton StopBtn;
    TextureButton PreviousBtn;
    TextureButton NextBtn;
    MenuButton RadioList;
    PopupMenu Radio;

    Label VolumeValue;
    HSlider VolumeBar;

    ToolButton AddStation;
    ConfirmationDialog NewStation;

    LineEdit StationName;
    LineEdit StationURL;

    public void LoadNodes()
    {
        StationHandler = (StationHandler)GetNode("/root/StationHandler");

        PlayContainer = GetNode<VBoxContainer>("HBoxContainer/VBoxContainer");
        StopContainer = GetNode<VBoxContainer>("HBoxContainer/VBoxContainer2");
        PlayBtn = GetNode<TextureButton>("HBoxContainer/VBoxContainer/Play");
        StopBtn = GetNode<TextureButton>("HBoxContainer/VBoxContainer2/Stop");
        PreviousBtn = GetNode<TextureButton>("HBoxContainer2/Previous");
        NextBtn = GetNode<TextureButton>("HBoxContainer2/Next");
        PreviousBtn.Connect("pressed",this,nameof(PreviousStation));
        RadioList = GetNode<MenuButton>("HBoxContainer2/MenuButton");
        VolumeValue = GetNode<Label>("HBoxContainer3/Value");
        VolumeBar = GetNode<HSlider>("HBoxContainer3/VolumeBar");
        AddStation = GetNode<ToolButton>("AddStation");
        NewStation = GetNode<ConfirmationDialog>("NewStation");

        StationName = GetNode<LineEdit>("NewStation/VBoxContainer/LineEdit");
        StationURL = GetNode<LineEdit>("NewStation/VBoxContainer/LineEdit2");

        Radio = RadioList.GetPopup();
    }

    public void ConnectSignals()
    {
        PlayBtn.Connect("pressed",this,nameof(PlayStream));
        StopBtn.Connect("pressed",this,nameof(StopStream));
        NextBtn.Connect("pressed",this,nameof(NextStation));
        Radio.Connect("index_pressed",this,nameof(RadioPressed));
        VolumeBar.Connect("value_changed",this,nameof(ChangeVolume));
        AddStation.Connect("pressed",this,nameof(PopupAddStation));
        NewStation.Connect("confirmed",this,nameof(AddNewStation));

        StationHandler.Connect(nameof(StationHandler.AddNewStation),this,nameof(AddRadioItem));
    }

    private void MyDownloadProc(IntPtr buffer, int length, IntPtr user)
    {
        if (buffer != IntPtr.Zero && length == 0)
        {
            // the buffer contains HTTP or ICY tags.
            // you might instead also use "this.BeginInvoke(...)", which would call the delegate asynchron!
        }
    }

    public override void _Ready()
    {
        LoadNodes();
        ConnectSignals();

        StopContainer.Hide();

        foreach (Dictionary radio in StationHandler.GetStationList())
        {
            AddRadioItem(radio);
        }
        
    }

    public void AddRadioItem(Dictionary radio)
    {
            Radio.AddItem(radio["strong"] as String);
            Radio.SetItemMetadata(RadioList.GetPopup().GetItemCount()-1, radio["code"]);        
    }

    public override void _Process(float delta)
    {
        
    }

    public void RadioPressed(int radio_index)
    {
        if (((String)Radio.GetItemMetadata(radio_index)).EndsWith(".m3u8")) {
            //StreamRadioM3U(radio_index);
        }   
        else {
            StreamRadioMediafoundation(radio_index);
        }
    }

    public void StreamRadioMediafoundation(int radio_index)
    {
        StopStream();
        try
        {
            mediaFoundationReader = new MediaFoundationReader(Radio.GetItemMetadata(radio_index) as String);
            RadioList.Text = Radio.GetItemText(radio_index);
            radioUrl = Radio.GetItemMetadata(radio_index) as String;
            radioName = Radio.GetItemText(radio_index);
            radioIndex = radio_index;
        }
        catch (System.Exception)
        {
            GD.Print("Could not stream ", Radio.GetItemText(radio_index), " station at ", Radio.GetItemMetadata(radio_index) as String);
        }
        waveOutEvent.Init(mediaFoundationReader);
        
        GD.Print("Reproducing ",radioName," on ",radioUrl);
        PlayStream();
    }

    public void StreamRadioM3U(int radio_index)
    {
    }

    public void PlayStream()
    {
        if (radioUrl!="") {
            waveOutEvent.Play();
            PlayContainer.Hide();
            StopContainer.Show();
        }

        
    }

    public void StopStream()
    {
        waveOutEvent.Stop();
        StopContainer.Hide();
        PlayContainer.Show();
    }

    public void PreviousStation()
    {
        if(radioIndex>0) {
            RadioPressed(radioIndex-1);
        }
    }

    public void NextStation()
    {
        if (radioIndex < StationHandler.GetStationList().Count-1) {
            RadioPressed(radioIndex+1);
        }
    }

    public void ChangeVolume(float value)
    {
        volume = value;
        VolumeValue.Text = volume.ToString();
        waveOutEvent.Volume = volume/100;
    }

    public void PopupAddStation()
    {
        NewStation.PopupCentered();
    }

    public void AddNewStation()
    {
        String SName = StationName.Text;
        String SURL = StationURL.Text;
        if (SName!="" && SURL!="")
        {
            StationHandler.AddStation(SName, SURL);
        }
    }
}
