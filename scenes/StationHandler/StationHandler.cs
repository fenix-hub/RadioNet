using Godot;
using System;
using Godot.Collections;

public class StationHandler : Godot.Node
{
    [Signal]
    public delegate void AddNewStation(Dictionary newRadio);

    private String stationsFilePath = "user://lista.json";
    private Godot.Collections.Array stationList = new Godot.Collections.Array();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        String[] list = {};
        String content = "";
        File file = new File();
        
        if (!file.FileExists(stationsFilePath))
        {
            file.Open("res://lista.json",File.ModeFlags.Read);
            content = file.GetAsText();
            file.Close();
            file.Open(stationsFilePath, File.ModeFlags.Write);
            file.StoreString(content);
            file.Close();
        }
        else 
        {
            file.Open(stationsFilePath, File.ModeFlags.Read);
            content = file.GetAsText();
            file.Close();
        }
        
        file.Open("res://lista.json",File.ModeFlags.Read);
        content = file.GetAsText();
        file.Close();
        list = content.TrimStart('[').TrimEnd(']').Split("::");
        foreach( String element in list ){
            stationList.Add(JSON.Parse(element).Result as Godot.Collections.Dictionary);
        }

        
    }

    public Godot.Collections.Array GetStationList()
    {
        return stationList;
    }

    public void AddStation(String stationName, String stationURL)
    {
        Godot.Collections.Dictionary newStation = new Godot.Collections.Dictionary();
        newStation.Add("strong",stationName);
        newStation.Add("code",stationURL);
        stationList.Add(newStation);
        
        File file = new File();
        file.Open(stationsFilePath,File.ModeFlags.ReadWrite);
        String content = file.GetAsText().TrimStart('[').TrimEnd(']');
        content+="["+JSON.Print(newStation)+"::]";
        file.StoreString(content);
        file.Close();
        EmitSignal(nameof(AddNewStation), newStation);
        

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
