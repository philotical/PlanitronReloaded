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
        string _planetName = "Kerbin";
        string _planetName_label = "";
        string _currentPlanetName;
        string _currentGee ;
        string _currentatmosphereMultiplier;
        bool _currentOxygen;
        double RoundedGeeASL;
        bool _loaded = false;



        Vessel vessel = new Vessel();
        CelestialBody _kerbin = new CelestialBody();
        double _gravity = 1.0;
        double _atmos = 1.0;
        Body[] Bodies = {
                            new Body("Sun", 1.746, 0 , 0, false, false,     0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Kerbin", 1.0, 1.0 , 0, true, true,    0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Mun", 0.166, 0 , 0, false, false,     0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Minmus", 0.05, 0 , 0, false, false,   0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Moho", 0.275, 0 , 0, false, false,    0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Eve", 1.7, 5 , 0, true, false,        0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Duna", 0.3, 0.2 , 0, true, true,      0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Ike", 0.112, 0 , 0, false, false,     0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Jool", 0.8, 15 , 0, true, false,      0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Laythe", 0.8, 0.8 , 0, true, true,    0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Val", 0.235, 0 , 0, false, false,     0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Bop", 0.06, 0 , 0, false, false,      0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Tylo", 0.8, 0 , 0, false, false,      0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Gilly", 0.005, 0 , 0, false, false,   0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Pol", 0.038, 0 , 0, false, false,     0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Dres", 0.115, 0 , 0, false, false,    0, 0, "", 0, 0, 0, 0, 0, 0),
                            new Body("Eeloo", 0.172,  0, 0, false, false,   0, 0, "", 0, 0, 0, 0, 0, 0)
                        };
        
        private Body currentBody;

        private float lastUpdate = 0.0f;
        private float logInterval = 20.0f;


        private static Rect windowPosition = new Rect(0, 0, 580, 580);
        private static Rect windowPosition2 = new Rect(0, 90, 90, 50);
        public Rect _windowRect = new Rect(0, 0, 580, 580);
        public Rect _windowRect2 = new Rect(0, 0, 80, 20);
        private static GUIStyle windowStyle = null;
        private static GUIStyle windowStyle2 = null;
        private static bool PlanitronWindowState = false;
        private static bool PlanitronbuttonState = true;


        public void Awake()
        {

            
            
            RenderingManager.AddToPostDrawQueue(0, OnDraw);

            Debug.Log("Planitron: Awake");
        }

        public void Start()
        {
            this.vessel = FlightGlobals.ActiveVessel;
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle2 = new GUIStyle(HighLogic.Skin.window);


            Debug.Log("Planitron: Start");
            currentBody = Bodies[1];
            Debug.Log("Planitron: Start: Set currentBody (Name) = " + currentBody.Name.ToString());
            Debug.Log("Planitron: Start: Set currentBody (type) = " + currentBody.GetType());

            this.vessel = FlightGlobals.ActiveVessel;
            //If we haven't saved our default kerbin variables, do so
            if (!_loaded)
            {
                Debug.Log("Planitron: Start: !_loaded call ModifyPlanet");
                RestoreKerbin(vessel.mainBody);
                _loaded = true;
            }
        }


        private void OnDraw()
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
            if ((Time.time - lastUpdate) > logInterval)
            {
                lastUpdate = Time.time;
                Debug.Log("Planitron: OnDraw");
            }

        }

        private void OnButton(int windowID)
        {
            Debug.Log("Planitron: OnButton Pre");
            GUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(6f, 28f, 80f, 20f), "Open"))
            {
                Debug.Log("Planitron: OnButton (TogglePlanitronWindow) clicked");
                TogglePlanitronWindow();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
/*
            */
            Debug.Log("Planitron: OnButton Post");
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

            GUIStyle Planitron_sty = new GUIStyle(GUI.skin.button);
            Planitron_sty.normal.textColor = Planitron_sty.focused.textColor = Color.white;
            Planitron_sty.hover.textColor = Planitron_sty.active.textColor = Color.yellow;
            Planitron_sty.onNormal.textColor = Planitron_sty.onFocused.textColor = Planitron_sty.onHover.textColor = Planitron_sty.onActive.textColor = Color.green;
            Planitron_sty.padding = Planitron_txtR.padding;
            Planitron_sty.margin = Planitron_txtR.margin;
            */
            //Ensure we are still on Kerbin
            if (vessel.mainBody.GetName() != "Kerbin")
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Planitron only works while on Kerbin");
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
                        if (_planetName == "Sun")
                        {
                            _planetName_label += " (I woudn't!)";
                        }
                        if (GUILayout.Button(_planetName_label))
                        {
                            if (_planetName == "Kerbin")
                            {
                                RestoreKerbin(vessel.mainBody);
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
                        if (_planetName == "Kerbin")
                        {
                            GUILayout.Label("1");
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
                    if (_planetName == "Kerbin")
                    {
                        GUILayout.Label("1");
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
            if (PlanitronWindowState)
            {
                PlanitronbuttonState = true;
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

                    /*
                    currentBody.Name = c.GetName();
                    currentBody.Gravity = c.GeeASL;
                    currentBody.Atmos = c.atmosphereMultiplier;

                    currentBody.atmoshpereTemperatureMultiplier = c.atmoshpereTemperatureMultiplier;
                    currentBody.atmosphere = c.atmosphere;
                    currentBody.atmosphereContainsOxygen = c.atmosphereContainsOxygen;
                    currentBody.atmosphereMultiplier = c.atmosphereMultiplier;
                    currentBody.atmosphereScaleHeight = c.atmosphereScaleHeight;
                    currentBody.atmosphericAmbientColor = c.atmosphericAmbientColor.ToString();

                    currentBody.GeeASL = c.GeeASL;
                    currentBody.gMagnitudeAtCenter = c.gMagnitudeAtCenter;
                    currentBody.gravParameter = c.gravParameter;
                    currentBody.Mass = c.Mass;
                    currentBody.pressureMultiplier = c.pressureMultiplier;
                    currentBody.staticPressureASL = c.staticPressureASL;
                    */


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
                //oldB.atmosphericAmbientColor = newB.atmosphericAmbientColor;

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

        private void RestoreKerbin(CelestialBody oldB)
        {
            Debug.Log("Planitron: Restore Kerbin Defaults");
            _currentPlanetName = "Kerbin";
            _currentGee = "1";
            _currentatmosphereMultiplier = "1";
            _currentOxygen = true;
            oldB.atmoshpereTemperatureMultiplier = (float)1;
            oldB.atmosphere = true;
            oldB.atmosphereContainsOxygen = true;
            oldB.atmosphereMultiplier = (float)1;
            oldB.atmosphereScaleHeight = (float)5;
            //oldB.atmosphericAmbientColor = "RGBA(0.243, 0.251, 0.255, 1.000)";

            oldB.GeeASL = 1;
            oldB.gMagnitudeAtCenter = 3531600000000;
            oldB.gravParameter = 3531600000000;
            oldB.Mass = 5.29157926281091E+22;
            oldB.pressureMultiplier = (float)0;
            oldB.staticPressureASL = (float)1;
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
