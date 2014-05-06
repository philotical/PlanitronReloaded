/*
 * Based on HackJeb Extension Planitron, originall by lukedupin
 * Ported to KSP 0.32.+ by philotical
 * 
 * License: GNU GPL - for more info see LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP.IO;



namespace Philotical
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class PlanitronReloaded : MonoBehaviour
    {
        //private ButtonWrapper PlanitronToolbarButton;
        //private IButton PlanitronToolbarButton;
        string _planetName = "";
        string _planetName_label = "";
        string _currentPlanetName;
        string _currentGee ;
        string _currentatmosphereMultiplier;
        bool _currentOxygen;
        double RoundedGeeASL;
        bool _loaded = false;


        string _defaultPlanetName;
        float  _defaultatmoshpereTemperatureMultiplier;
        bool   _defaultatmosphere;
        bool   _defaultatmosphereContainsOxygen;
        float  _defaultatmosphereMultiplier;
        float  _defaultatmosphereScaleHeight;
        float  atmosphericAmbientColor_col_R;
        float  atmosphericAmbientColor_col_G;
        float  atmosphericAmbientColor_col_B;
        float  atmosphericAmbientColor_col_A;
        float  _defaultGeeASL;
        float  _defaultgMagnitudeAtCenter;
        float  _defaultgravParameter;
        float  _defaultMass;
        float  _defaultpressureMultiplier;
        float  _defaultstaticPressureASL;


        float col_R;
        float col_G;
        float col_B;
        float col_A;
        UnityEngine.Color col_RGBA;

        Vessel vessel = new Vessel();
        CelestialBody _kerbin = new CelestialBody();
        double _gravity = 1.0;
        double _atmos = 1.0;

        private float lastUpdate = 0.0f;
        private float logInterval = 20.0f;


        private static Rect windowPosition = new Rect(10, 10, 580, 580);
        private static Rect windowPosition2 = new Rect(10, 90, 54, 54);
        private static GUIStyle windowStyle = null;
        private static GUIStyle windowStyle2 = null;
        private static bool PlanitronWindowState = false;
        private static bool PlanitronbuttonState = false;

        PluginConfiguration cfg = PluginConfiguration.CreateForType<PlanitronReloaded>();

        private GUIContent content;
        private GUIStyle iconStyle;

        public void Awake()
        {

            
            
            RenderingManager.AddToPostDrawQueue(0, OnDraw);

            Debug.Log("Planitron: Awake");
        }

        public bool IsPrelaunch()
        {
            return vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.LANDED;
        }


        public void Start()
        {
            Debug.Log("Planitron: Start");
            this.vessel = FlightGlobals.ActiveVessel;
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle2 = new GUIStyle(HighLogic.Skin.window);


            /*
            PlanitronToolbarButton = new ButtonWrapper(new Rect(Screen.width * 0.7f, 0, 32, 32),
                "Philotical/Planitron/Icons/planitron_Icon", "PT",
                "TAC Fuel Balancer", TogglePlanitronWindow);
            
            if (ToolbarManager.ToolbarAvailable)
            {
                // regular button
                PlanitronToolbarButton = ToolbarManager.Instance.add("test", "PlanitronToolbarButton");
                PlanitronToolbarButton.TexturePath = "Philotical/Planitron/Icons/planitron_Icon";
                PlanitronToolbarButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                PlanitronToolbarButton.ToolTip = "Planitron Reloaded";
                PlanitronToolbarButton.OnClick += (e) => TogglePlanitronWindow();
            }
            */

            if (!FlightGlobals.ready)
            {
                Debug.Log("Planitron: FlightGlobals are not valid yet. Hiding the Button");
                //PlanitronToolbarButton.Visible = !PlanitronToolbarButton.Visible;
            }

            this.vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                Debug.Log("Planitron Abort: No active vessel yet.");
                return;
            }

            if (vessel.isEVA)
            {
                Debug.Log("Planitron Abort: Vessel is EVA.");
                return;
            }
            if (!IsPrelaunch())
            {
                Debug.Log("Planitron Abort: !IsPrelaunch");
                return;
            }






            Debug.Log("Planitron: Populate Config File with current Bodies Defaults");
            cfg.load();
            windowPosition = cfg.GetValue<Rect>("windowpos");
            windowPosition2 = cfg.GetValue<Rect>("buttonpos");


            foreach (CelestialBody c in FlightGlobals.Bodies)
            {
                if (c.GetName() == vessel.mainBody.GetName())
                {
                    Debug.Log("Planitron: Active Default Body on Startup is: " + c.GetName());
                    //If we haven't saved our default Planet variables, do so
                    cfg["_defaultPlanetName"] = c.GetName();
                    cfg["_defaultatmoshpereTemperatureMultiplier"] = c.atmoshpereTemperatureMultiplier.ToString();
                    cfg["_defaultatmosphere"] = c.atmosphere;
                    cfg["_defaultatmosphereContainsOxygen"] = c.atmosphereContainsOxygen;
                    cfg["_defaultatmosphereMultiplier"] = c.atmosphereMultiplier.ToString();
                    cfg["_defaultatmosphereScaleHeight"] = c.atmosphereScaleHeight;
                    cfg["atmosphericAmbientColor_col_R"] = c.atmosphericAmbientColor.r.ToString();
                    cfg["atmosphericAmbientColor_col_G"] = c.atmosphericAmbientColor.g.ToString();
                    cfg["atmosphericAmbientColor_col_B"] = c.atmosphericAmbientColor.b.ToString();
                    cfg["atmosphericAmbientColor_col_A"] = c.atmosphericAmbientColor.a.ToString();
                    cfg["_defaultGeeASL"] = c.GeeASL;
                    cfg["_defaultgMagnitudeAtCenter"] = c.gMagnitudeAtCenter;
                    cfg["_defaultgravParameter"] = c.gravParameter;
                    cfg["_defaultMass"] = c.Mass;
                    cfg["_defaultpressureMultiplier"] = c.pressureMultiplier.ToString();
                    cfg["_defaultstaticPressureASL"] = c.staticPressureASL;
                    cfg["windowpos"] = windowPosition;
                    cfg["buttonpos"] = windowPosition2;
                    cfg.save();
                    }
            }
            Debug.Log("Planitron: Config File updated");
            Debug.Log("Planitron: New Planetary Values:");
            Debug.Log("Planitron: _defaultPlanetName = " + cfg["_defaultPlanetName"]);
            Debug.Log("Planitron: _defaultatmoshpereTemperatureMultiplier = " + cfg["_defaultatmoshpereTemperatureMultiplier"]);
            Debug.Log("Planitron: _defaultatmosphere = " + cfg["_defaultatmosphere"]);
            Debug.Log("Planitron: _defaultatmosphereContainsOxygen = " + cfg["_defaultatmosphereContainsOxygen"]);



            if (!_loaded)
            {
                Debug.Log("Planitron: Start: !_loaded call ModifyPlanet");
                Restore_defaultPlanetName(vessel.mainBody);
                _loaded = true;
            }
        }

        internal void OnDestroy()
        {
            cfg["windowpos"] = windowPosition;
            cfg["buttonpos"] = windowPosition2;
            cfg.save();
            //PlanitronToolbarButton.Destroy();
        }


        private void OnDraw()
        {
            if ((Time.time - lastUpdate) > logInterval)
            {
                lastUpdate = Time.time;
                Debug.Log("Planitron: OnDraw");
            }

            /*
            if (FlightGlobals.ready && !PlanitronToolbarButton.Visible)
            {
                Debug.Log("Planitron: FlightGlobals are valid now. Activating Button");
                PlanitronToolbarButton.Visible = true;
            }

            */
            if (FlightGlobals.ready)
            {
                if ((Time.time - lastUpdate) > logInterval)
                {
                    Debug.Log("Planitron: FlightGlobals are valid now. Activating Button");
                }
                PlanitronbuttonState = true;
            }
            if (FlightGlobals.ready && !PlanitronWindowState && !PlanitronbuttonState)
            {
                PlanitronbuttonState = true;
            }
            if (FlightGlobals.ready && PlanitronWindowState)
            {
                windowPosition = GUI.Window(9874, windowPosition, OnWindow, "Planitron Reloaded", windowStyle);
                PlanitronbuttonState = false;
            }
            else
            {

            }
            if (vessel.mainBody.GetName() != _defaultPlanetName)
            {
                PlanitronbuttonState = false;
            }
            if (FlightGlobals.ready && PlanitronbuttonState)
            {
                /*
                if (ToolbarManager.ToolbarAvailable)
                {
                    PlanitronWindowState = false;
                }
                else 
                {
                    if (!PlanitronWindowState && !PlanitronbuttonState)
                    {
                        PlanitronbuttonState = true;
                    }
                    if (PlanitronWindowState)
                    {
                        windowPosition = GUI.Window(9874, windowPosition, OnWindow, "Planitron Reloaded", windowStyle);
                        PlanitronbuttonState = false;
                    }
                    else
                    {

                    }
                    if (PlanitronbuttonState)
                    {
                        windowPosition2 = GUI.Window(9875, windowPosition2, OnButton, "Planitron", windowStyle2);
                        PlanitronWindowState = false;
                    }
                }
                */
                    if (!PlanitronWindowState && !PlanitronbuttonState && vessel.mainBody.GetName() == _defaultPlanetName)
                    {
                        PlanitronbuttonState = true;
                    }
                    if (PlanitronWindowState)
                    {
                        windowPosition = GUI.Window(9874, windowPosition, OnWindow, "Planitron Reloaded", windowStyle);
                        PlanitronbuttonState = false;
                    }
                    else
                    {

                    }
                    if (PlanitronbuttonState)
                    {                        
                        windowPosition2 = GUI.Window(9875, windowPosition2, OnButton, "", windowStyle2);
                        PlanitronWindowState = false;
                        if (iconStyle == null)
                        {
                            iconStyle = new GUIStyle(GUI.skin.button);
                            iconStyle.alignment = TextAnchor.MiddleCenter;
                            iconStyle.padding = new RectOffset(1, 1, 1, 1);
                        }
                    }
            }
        }

        private void OnButton(int windowID)
        {
            GUILayout.BeginHorizontal();
            Texture2D texture = GameDatabase.Instance.GetTexture("Philotical/Planitron/Icons/planitron_Icon", false);
            content = new GUIContent(texture, "Planitron");
            //GUI.Label(windowPosition2, content, iconStyle);

            if (GUI.Button(new Rect(7f, 7f, 40f, 40f), content, iconStyle))
            {
                Debug.Log("Planitron: OnButton (TogglePlanitronWindow) clicked");
                TogglePlanitronWindow();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void OnWindow(int windowID)
        {
        
            /*
            GUIStyle Planitron_txtR = new GUIStyle(GUI.skin.label);
            Planitron_txtR.alignment = TextAnchor.UpperRight;
            GUIStyle Planitron_txtR_Ex = new GUIStyle(GUI.skin.label);
            Planitron_txtR_Ex.alignment = TextAnchor.UpperRight;
            Planitron_txtR_Ex.stretchWidth = true;
            Planitron_txtR_Ex.margin.right = 16;
            */

            GUIStyle Planitron_sty = new GUIStyle(GUI.skin.button);
            Planitron_sty.normal.textColor = Planitron_sty.active.textColor = Color.white;
            Planitron_sty.hover.textColor = Planitron_sty.focused.textColor = Color.yellow;

            GUIStyle Planitron_sty_selected = new GUIStyle(GUI.skin.button);
            Planitron_sty_selected.normal.textColor = Planitron_sty_selected.active.textColor = Color.green;
            Planitron_sty_selected.hover.textColor = Planitron_sty_selected.focused.textColor = Color.yellow;

            //Ensure we are still on Kerbin
            if (vessel.mainBody.GetName() != _defaultPlanetName)
            {
                GUILayout.BeginHorizontal();
                if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
                {
                    TogglePlanitronWindow();
                }
                GUILayout.BeginVertical();
                GUILayout.Label("Planitron only works while on " + _defaultPlanetName);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUI.DragWindow();
                return;
            }
            
            GUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                TogglePlanitronWindow();
            }
            //Planet selector
                GUILayout.BeginVertical();
                GUILayout.Label("Celestial Body");
                    foreach (CelestialBody c in FlightGlobals.Bodies)
                    {
                        _planetName = c.GetName();
                        _planetName_label = _planetName;
                        if (c.atmoshpereTemperatureMultiplier> 10)
                        {
                            _planetName_label += " (may be too hot)";
                        }

                        GUIStyle Planitron_sty_used = (_planetName == _currentPlanetName) ? Planitron_sty_selected : Planitron_sty;
                        if (GUILayout.Button(_planetName_label, Planitron_sty_used))
                        {
                            if (_planetName == _defaultPlanetName)
                            {
                                    Restore_defaultPlanetName(vessel.mainBody);
                                    Debug.Log("Planitron: RestoreKerbin Button clicked: " + _planetName);
                            }
                            else 
                            {
                                SetPlanet(_planetName);
                                Debug.Log("Planitron: Planet Button clicked: " + _planetName);
                            }
                        }
                    }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("G's");
                    foreach (CelestialBody c in FlightGlobals.Bodies)
                    {
                        _planetName = c.GetName();
                        if (_planetName == _defaultPlanetName)
                        {
                            GUILayout.Label(_defaultGeeASL.ToString());
                        }
                        else
                        {
                            GUILayout.Label(c.GeeASL.ToString("N3"));
                        }
                    }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Atmosphere");
                foreach (CelestialBody c in FlightGlobals.Bodies)
                {
                    _planetName = c.GetName();
                    if (_planetName == _defaultPlanetName)
                    {
                        GUILayout.Label(_defaultatmosphereMultiplier.ToString());
                    }
                    else
                    {
                        if (c.atmosphere)
                        {
                            GUILayout.Label(c.atmosphereMultiplier.ToString("N3"));
                        }
                        else
                        {
                            GUILayout.Label("None");
                        }
                    }
                }
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Body:");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:");
            GUILayout.Label("Gee:");
            GUILayout.Label("Atmosphere:");
            GUILayout.Label("Oxygene:");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(_currentPlanetName);
            GUILayout.Label(_currentGee);
            GUILayout.Label(_currentatmosphereMultiplier);
            GUILayout.Label(_currentOxygen.ToString());

            GUILayout.EndHorizontal();


           GUI.DragWindow();
            /*
            */
        }
        private void TogglePlanitronWindow()
        {
            Debug.Log("Planitron: TogglePlanitronWindow");
            if (PlanitronWindowState)
            {
                if (vessel.mainBody.GetName() != _defaultPlanetName)
                {
                    PlanitronbuttonState = true;
                }
                PlanitronWindowState = false;
            }
            else
            {
                PlanitronbuttonState = false;
                PlanitronWindowState = true;
            }
        }

         private void SetPlanet(string _planetName)
        {

            foreach (CelestialBody c in FlightGlobals.Bodies)
            {
                if (_planetName == c.GetName())
                {


                    Debug.Log("Planitron: SetPlanet: pre call ModifyPlanet");
                    Debug.Log("Planitron: SetPlanet: pre call ModifyPlanet Vessel.mainBody.GetNam() = " + vessel.mainBody.GetName());
                    Debug.Log("Planitron: SetPlanet: pre call ModifyPlanet c.GetNam() = " + c.GetName());
                    ModifyPlanet(vessel.mainBody, c);
                    Debug.Log("Planitron: SetPlanet: post call ModifyPlanet");
                    Debug.Log(" ");
                    Debug.Log(" ");
                }
            }
                
        }

        private void ModifyPlanet(CelestialBody oldB, CelestialBody newB)
        {
            Debug.Log("Planitron: ModifyPlanet oldB.GetName() =  " + oldB.GetName());
            Debug.Log("Planitron: ModifyPlanet newB.GetName() =  " + newB.GetName());

            _currentPlanetName = newB.GetName();
            RoundedGeeASL = newB.GeeASL;
            RoundedGeeASL = Math.Round(RoundedGeeASL, 2);
            _currentGee = RoundedGeeASL.ToString();
            if (newB.atmosphere)
            {
                _currentatmosphereMultiplier = newB.atmosphereMultiplier.ToString();
            }
            else
            {
                _currentatmosphereMultiplier = "None";
            }
            _currentOxygen = newB.atmosphereContainsOxygen;
                oldB.atmoshpereTemperatureMultiplier = newB.atmoshpereTemperatureMultiplier;
                oldB.atmosphere = newB.atmosphere;
                oldB.atmosphereContainsOxygen = newB.atmosphereContainsOxygen;
                oldB.atmosphereMultiplier = newB.atmosphereMultiplier;
                oldB.atmosphereScaleHeight = newB.atmosphereScaleHeight;
                

                col_R = newB.atmosphericAmbientColor.r;
                col_G = newB.atmosphericAmbientColor.g;
                col_B = newB.atmosphericAmbientColor.b;
                col_A = newB.atmosphericAmbientColor.a;
                col_RGBA = new Color(col_R, col_G, col_B, col_A);
                oldB.atmosphericAmbientColor = col_RGBA;

                oldB.GeeASL = newB.GeeASL;
                oldB.gMagnitudeAtCenter = newB.gMagnitudeAtCenter;
                oldB.gravParameter = newB.gravParameter;
                oldB.Mass = newB.Mass;
                oldB.pressureMultiplier = newB.pressureMultiplier;
                oldB.staticPressureASL = newB.staticPressureASL;
            Debug.Log(" ");
            Debug.Log("Planitron: ModifyPlanet: New Values for vessel.mainBody ----------------------------");
            Debug.Log("Planitron: ModifyPlanet Post Name =  " + oldB.GetName());

            Debug.Log("Planitron: ModifyPlanet Post atmoshpereTemperatureMultiplier =  " + oldB.atmoshpereTemperatureMultiplier);
            Debug.Log("Planitron: ModifyPlanet Post atmosphere =  " + oldB.atmosphere);
            Debug.Log("Planitron: ModifyPlanet Post atmosphereContainsOxygen =  " + oldB.atmosphereContainsOxygen);
            Debug.Log("Planitron: ModifyPlanet Post atmosphereMultiplier =  " + oldB.atmosphereMultiplier);
            Debug.Log("Planitron: ModifyPlanet Post atmosphereScaleHeight =  " + oldB.atmosphereScaleHeight);
            Debug.Log("Planitron: ModifyPlanet Post col_RGBA.ToString =  " + col_RGBA.ToString());
            Debug.Log("Planitron: ModifyPlanet Post col_RGBA =  " + col_RGBA);
            Debug.Log("Planitron: ModifyPlanet Post atmosphericAmbientColor =  " + oldB.atmosphericAmbientColor.ToString());
            Debug.Log("Planitron: ModifyPlanet Post GeeASL =  " + oldB.GeeASL);
            Debug.Log("Planitron: ModifyPlanet Post gMagnitudeAtCenter =  " + oldB.gMagnitudeAtCenter);
            Debug.Log("Planitron: ModifyPlanet Post gravParameter =  " + oldB.gravParameter);
            Debug.Log("Planitron: ModifyPlanet Post Mass =  " + oldB.Mass);
            Debug.Log("Planitron: ModifyPlanet Post pressureMultiplier =  " + oldB.pressureMultiplier);
            Debug.Log("Planitron: ModifyPlanet Post staticPressureASL =  " + oldB.staticPressureASL);
            Debug.Log(" ");
            Debug.Log(" ");
        }

        private void Restore_defaultPlanetName(CelestialBody oldB)
        {
            Debug.Log("Planitron: Restore MainBody Defaults");


            cfg.load();
            _defaultPlanetName = cfg.GetValue<string>("_defaultPlanetName");
            _defaultatmoshpereTemperatureMultiplier = cfg.GetValue<float>("_defaultatmoshpereTemperatureMultiplier");
            _defaultatmosphere = cfg.GetValue<bool>("_defaultatmosphere");
            _defaultatmosphereContainsOxygen = cfg.GetValue<bool>("_defaultatmosphereContainsOxygen");
            _defaultatmosphereMultiplier = Convert.ToSingle(double.Parse(cfg.GetValue<string>("_defaultatmosphereMultiplier"))); //float
            _defaultatmosphereScaleHeight = Convert.ToSingle(cfg.GetValue<double>("_defaultatmosphereScaleHeight")); //float
            atmosphericAmbientColor_col_R = Convert.ToSingle(double.Parse(cfg.GetValue<string>("atmosphericAmbientColor_col_R"))); //float
            atmosphericAmbientColor_col_G = Convert.ToSingle(double.Parse(cfg.GetValue<string>("atmosphericAmbientColor_col_G"))); //float
            atmosphericAmbientColor_col_B = Convert.ToSingle(double.Parse(cfg.GetValue<string>("atmosphericAmbientColor_col_B"))); //float
            atmosphericAmbientColor_col_A = Convert.ToSingle(double.Parse(cfg.GetValue<string>("atmosphericAmbientColor_col_A"))); //float
            _defaultGeeASL = Convert.ToSingle(cfg.GetValue<double>("_defaultGeeASL")); //float
            _defaultgMagnitudeAtCenter = Convert.ToSingle(cfg.GetValue<double>("_defaultgMagnitudeAtCenter")); //float
            _defaultgravParameter = Convert.ToSingle(cfg.GetValue<double>("_defaultgravParameter")); //float
            _defaultMass = Convert.ToSingle(cfg.GetValue<double>("_defaultMass")); //float
            _defaultpressureMultiplier = Convert.ToSingle(double.Parse(cfg.GetValue<string>("_defaultpressureMultiplier"))); //float
            _defaultstaticPressureASL = Convert.ToSingle(cfg.GetValue<double>("_defaultstaticPressureASL")); //float
            
            _currentPlanetName = _defaultPlanetName;
            _currentGee = _defaultGeeASL.ToString();
            _currentatmosphereMultiplier = _defaultatmosphereMultiplier.ToString();
            _currentOxygen = _defaultatmosphereContainsOxygen;
            oldB.atmoshpereTemperatureMultiplier = _defaultatmoshpereTemperatureMultiplier;
            oldB.atmosphere = _defaultatmosphere;
            oldB.atmosphereContainsOxygen = _defaultatmosphereContainsOxygen;
            oldB.atmosphereMultiplier = (float)_defaultatmosphereMultiplier;
            oldB.atmosphereScaleHeight = (float)_defaultatmosphereScaleHeight;
            //oldB.atmosphericAmbientColor = "RGBA(0.243, 0.251, 0.255, 1.000)";


            col_R = atmosphericAmbientColor_col_R;
            col_G = atmosphericAmbientColor_col_G;
            col_B = atmosphericAmbientColor_col_B;
            col_A = atmosphericAmbientColor_col_A;
            col_RGBA = new Color(col_R, col_G, col_B, col_A);
            oldB.atmosphericAmbientColor = col_RGBA;


            oldB.GeeASL = _defaultGeeASL;
            oldB.gMagnitudeAtCenter = _defaultgMagnitudeAtCenter;
            oldB.gravParameter = _defaultgravParameter;
            oldB.Mass = _defaultMass;
            oldB.pressureMultiplier = (float)_defaultpressureMultiplier;
            oldB.staticPressureASL = (float)_defaultstaticPressureASL;
            Debug.Log("Planitron: new _defaultPlanetName = " + _defaultPlanetName);

            windowPosition = cfg.GetValue<Rect>("windowpos");
            windowPosition2 = cfg.GetValue<Rect>("buttonpos");

        }



        //Holds body info
        public class Body
        {
            public string Name;
            public double Gravity;
            public double Atmos;

            public double atmoshpereTemperatureMultiplier;
            public bool atmosphere;
            public bool atmosphereContainsOxygen ;
            public double atmosphereMultiplier;
            public double atmosphereScaleHeight;
            public string atmosphericAmbientColor; 


            public double GeeASL;
            public double gMagnitudeAtCenter;
            public double gravParameter;
            public double Mass;
            public double pressureMultiplier;
            public double staticPressureASL;



            public Body(string name, double g, double a
                , double atmoshpereTemperatureMultiplier
                , bool atmosphere
                , bool atmosphereContainsOxygen
                , double atmosphereMultiplier
                , double atmosphereScaleHeight
                , string atmosphericAmbientColor
                , double GeeASL
                , double gMagnitudeAtCenter
                , double gravParameter
                , double Mass
                , double pressureMultiplier
                , double staticPressureASL
                )
            {
                Debug.Log("Planitron: Body: " + name);
                Name = name;
                Gravity = g;
                Atmos = a;

                atmoshpereTemperatureMultiplier = atmoshpereTemperatureMultiplier;
                atmosphere = atmosphere;
                atmosphereContainsOxygen = atmosphereContainsOxygen;
                atmosphereMultiplier = atmosphereMultiplier;
                atmosphereScaleHeight = atmosphereScaleHeight;
                atmosphericAmbientColor = atmosphericAmbientColor;

                GeeASL = GeeASL;
                gMagnitudeAtCenter = gMagnitudeAtCenter;
                gravParameter = gravParameter;
                Mass = Mass;
                pressureMultiplier = pressureMultiplier;
                staticPressureASL = staticPressureASL;
            }

        }


        public string getPlanet()
        {
            Debug.Log("Planitron: getPlanet: " + _planetName);
            return _planetName;
        }


        public double gravity()
        {
            Debug.Log("Planitron: gravity");
            return _gravity;
        }

        public double atmos()
        {
            Debug.Log("Planitron: atmos");
            return _atmos;
        }


    }
}
