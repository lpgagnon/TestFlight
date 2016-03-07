﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TestFlightAPI;

public class EngineModuleWrapper
{
    public enum EngineModuleType
    {
        UNKNOWN = -1,
        ENGINE,
        SOVLERENGINE
    }

    public enum EngineIgnitionState
    {
        UNKNOWN = -1,
        NOT_IGNITED,
        IGNITED,
    }

    ModuleEngines engine;
    public EngineModuleType engineType;

    // Used to store the original fuel flow values
    private float _minFuelFlow;
    private float _maxFuelFlow;
    private float _g;

    // Public methods
    public PartModule Module
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return null;

            return engine as PartModule;
        }
    }

    public EngineModuleType EngineType
    {
        get { return engineType; }
    }

    public bool allowShutdown
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return false;

            return engine.allowShutdown;
        }
        set
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return;
            engine.allowShutdown = value;
        }
    }

    public bool throttleLocked
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return false;
            return engine.throttleLocked;
        }
        set
        {
            engine.throttleLocked = value;
        }
    }

    public float minFuelFlow
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;
            return engine.minFuelFlow;
        }
        set
        {
            engine.minFuelFlow = value;
        }
    }

    public float maxFuelFlow
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;
            return engine.maxFuelFlow;
        }
        set
        {
            engine.maxFuelFlow = value;
        }
    }

    public float g
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;
            return engine.g;
        }
        set
        {
            engine.g = value;
        }
    }

    public float maxThrust
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;
            return engine.maxThrust;
        }
        set
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return;
            engine.maxThrust = value;
        }
    }

    public float minThrust
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;

            return engine.minThrust;
        }
        set
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return;

            engine.minThrust = value;
        }
    }

    public bool flameout
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return false;

            return engine.flameout;
        }
        set
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return;
            engine.flameout = value;
        }
    }

    public bool enabled
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return false;

            return engine.enabled;
        }
        set
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return;

            engine.enabled = value;
        }
    }

    // DEPRECATED no longer an engine property in KSP 1.0
    public float requestedThrust
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;
            
            return 0f;
        }
    }

    public float finalThrust
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return 0f;

            return engine.finalThrust;
        }
    }

    public bool EngineIgnited
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return false;
            
            return engine.EngineIgnited;
        }
    }

    public BaseEventList Events
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return null;

            return engine.Events;
        }
    }

    public EngineIgnitionState IgnitionState
    {
        get
        {
            if (engineType == EngineModuleType.UNKNOWN)
                return EngineIgnitionState.UNKNOWN;

            if (engine.finalThrust > 0f)
                return EngineIgnitionState.IGNITED;

            return EngineIgnitionState.NOT_IGNITED;
        }
    }

    // "Shutdown Engine"
    public void Shutdown()
    {
        if (engineType == EngineModuleType.UNKNOWN)
            return;

        engine.Shutdown();
        engine.DeactivateRunningFX();
        engine.DeactivatePowerFX();
    }

    // Reduce fuel flow
    public void SetFuelFlowMult(float multiplier)
    {
        if (engineType == EngineModuleType.UNKNOWN)
            return;
        if (engineType == EngineModuleType.SOVLERENGINE)
        {
            engine.GetType().GetField("flowMult").SetValue(engine, multiplier);
        }
        else
        {
            engine.minFuelFlow = _minFuelFlow * multiplier;
            engine.maxFuelFlow = _maxFuelFlow * multiplier;
        }
    }

    public void SetFuelIspMult(float multiplier)
    {
        if (engineType == EngineModuleType.UNKNOWN)
            return;
        if (engineType == EngineModuleType.SOVLERENGINE)
        {
            engine.GetType().GetField("ispMult").SetValue(engine, multiplier);
        }
        else
        {
            engine.g = _g * multiplier;
        }
    }

    public EngineModuleWrapper()
    {
    }

    public void Init(Part part)
    {
        InitWithEngine(part, "");
    }

    public void InitWithEngine(Part part, string engineID)
    {
        ModuleEngines _engine = null;
        foreach (PartModule pm in part.Modules)
        {
            _engine = pm as ModuleEngines;
            if (_engine != null && (engineID == "" || _engine.engineID.ToLowerInvariant() == engineID.ToLowerInvariant()))
                break;
        }
        if (_engine != null)
        {
            engine = _engine;
            string tName = engine.GetType().Name;
            if (tName == "ModuleEnginesRF" || tName.Contains("ModuleEnginesAJE"))
                engineType = EngineModuleType.SOVLERENGINE;
            else
                engineType = EngineModuleType.ENGINE;

            _minFuelFlow = engine.minFuelFlow;
            _maxFuelFlow = engine.maxFuelFlow;
            _g = engine.g;
        }
        else
        {
            engineType = EngineModuleType.UNKNOWN;
        }
    }

    ~EngineModuleWrapper()
    {
    }

    internal void Log(string message)
    {
        PartModule pm = this.Module;
        if (pm == null)
            return;
        Part part = pm.part;
        if (part == null)
            return;
        string meType = "UNKNOWN";
        if (EngineType == EngineModuleType.ENGINE)
            meType = "ENGINE";
        if (EngineType == EngineModuleType.SOVLERENGINE)
            meType = "SOLVERENGINE";

        message = String.Format("TestFlight_EngineModuleWrapper({0}[{1}]): {2}", TestFlightUtil.GetFullPartName(part), meType, message);
        TestFlightUtil.Log(message, part);
    }
}
