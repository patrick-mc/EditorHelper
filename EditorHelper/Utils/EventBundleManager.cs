﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADOFAI;
using GDMiniJSON;
using MoreEditorOptions.Util;
using UnityModManagerNet;

namespace EditorHelper.Utils {
    public struct EventBundle {
        public List<string> LevelEvents;
        public string Name;
        public string Author;

        public EventBundle(string name, string author, List<LevelEvent> events) {
            Name = name;
            Author = author;
            LevelEvents = new List<string>();
            foreach (var evnt in events) {
                LevelEvents.Add(evnt.Encode(false).Replace("\t", "").Replace("  ", " "));
            }
        }

        public EventBundle(Dictionary<string, object> data) : this() {
            Decode(data);
        }

        public string Encode() {
            var stringBuilder = new StringBuilder();
            foreach (var evnt in LevelEvents) {
                stringBuilder.Append(
                    string.Concat(new string[] {
                            "        ",
                            "{ ",
                            evnt,
                            "},",
                            "\n"
                        }
                    ));
            }

            var result = $@"{{
    ""name"": ""{Name}"",
    ""author"": ""{Author}"",
    ""events"": [
{stringBuilder}
    ]
}}";
            return result;
        }

        public void Decode(Dictionary<string, object> data) {
            Name = (string) data["name"];
            Author = (string) data["author"];
            LevelEvents = new List<string>();
            foreach (Dictionary<string, object> evnt in (List<object>) data["events"]) {
                LevelEvents.Add(new LevelEvent(evnt).Encode(false).Replace("\t", "").Replace("  ", " "));
            }
        }
    }

    public static class EventBundleManager {

        public static List<EventBundle> Datas = new List<EventBundle>();

        public static void Save() {
            Main.Settings.EventBundles = Json.Serialize(Datas.Select(data => data.Encode()).ToList());
        }

        public static void Load() {
            try {
                Datas = ((List<object>) Json.Deserialize(Main.Settings.EventBundles))
                    .Select(o => new EventBundle((Dictionary<string, object>) o)).ToList();
            } catch {
                Datas = new List<EventBundle>();
            }
        }

        public static void ApplyBundle(this scnEditor instance, int floor, EventBundle data) {
            LevelEvent first = null;
            foreach (var levelevent in data.LevelEvents.Select(evnt =>
                new LevelEvent((Dictionary<string, object>) Json.Deserialize($"{{{evnt}}}")))) {
                if (first == null) first = levelevent;
                levelevent.floor = floor;
                instance.events.Add(levelevent);
                if (levelevent.eventType is LevelEventType.AddDecoration or LevelEventType.AddText) {
                    instance.set("refreshDecSprites", true);
                }

            }

            var eventType = first.eventType;
            var sequenceID = floor;
            scnEditor.instance.levelEventsPanel.selectedEventType = eventType;
            int count = scnEditor.instance.events
                .FindAll((LevelEvent x) => x.eventType == eventType && x.floor == sequenceID).Count;
            if (count == 1) {
                scnEditor.instance.DecideInspectorTabsAtSelected();
                scnEditor.instance.levelEventsPanel.ShowPanel(eventType, 0);
            } else {
                scnEditor.instance.levelEventsPanel.ShowPanel(eventType, count - 1);
            }

            scnEditor.instance.ApplyEventsToFloors();
            scnEditor.instance.ShowEventIndicators(scnEditor.instance.selectedFloors[0]);
            scnEditor.instance.ApplyEventsToFloors();
        }
    }
}