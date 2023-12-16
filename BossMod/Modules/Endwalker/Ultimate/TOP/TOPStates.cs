﻿using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class TOPStates : StateMachineBuilder
    {
        private TOP _module;

        private bool IsEffectivelyDead(Actor? actor) => actor != null && !actor.IsTargetable && actor.HP.Cur <= 1;

        public TOPStates(TOP module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1, "P1: Beetle")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2, "P2: M/F")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true) || IsEffectivelyDead(_module.BossP2M()) && IsEffectivelyDead(_module.BossP2F());
            SimplePhase(2, Phase3, "P3: Final")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true) || IsEffectivelyDead(_module.BossP3());
            SimplePhase(3, Phase4, "P4: Blue Screen")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true) || _module.FindComponent<P4BlueScreen>()?.NumCasts > 0;
            SimplePhase(4, Phase5, "P5")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true); // TODO: reconsider condition...
        }

        private void Phase1(uint id)
        {
            P1ProgramLoop(id, 10.1f);
            P1Pantokrator(id + 0x10000, 8.2f);
            P1WaveCannons(id + 0x20000, 6.6f);
            ActorCast(id + 0x30000, _module.BossP1, AID.AtomicRay, 5.8f, 5, true, "Enrage");
        }

        private void Phase2(uint id)
        {
            P2FirewallSolarRay(id, 3.4f);
            P2PartySynergy(id + 0x10000, 13.4f);
            P2LimitlessSynergy(id + 0x20000, 9.2f);
        }

        private void Phase3(uint id)
        {
            P3Intermission(id, 9.4f);
            P3HelloWorld(id + 0x10000, 4.1f);
            P3OversampledWaveCannon(id + 0x20000, 11.6f);
            ActorCast(id + 0x30000, _module.BossP3, AID.IonEfflux, 5.8f, 10, true, "Enrage");
        }

        private void Phase4(uint id)
        {
            P4WaveCannon(id, 7.2f);
            P4BlueScreen(id + 0x10000, 1.1f);
        }

        private void Phase5(uint id)
        {
            ActorTargetable(id, _module.BossP5, true, 15.5f, "Boss appears");
            P5SolarRay(id + 0x10000, 3.1f, false);
            P5RunMiDelta(id + 0x20000, 8.4f);
            P5SolarRay(id + 0x30000, 9.1f, false);
            P5RunMiSigma(id + 0x40000, 8.4f);
            P5SolarRay(id + 0x50000, 4.1f, true);
            P5RunMiOmega(id + 0x60000, 8.5f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void P1ProgramLoop(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.ProgramLoop, delay, 4, true)
                .ActivateOnEnter<P1ProgramLoop>();
            ActorCast(id + 0x10, _module.BossP1, AID.Blaster, 6.1f, 7.9f, true);
            // note: tethers explode ~0.1s after each tower set
            ComponentCondition<P1ProgramLoop>(id + 0x20, 0.1f, comp => comp.NumTowersDone >= 2, "Towers 1/tethers 3");
            ComponentCondition<P1ProgramLoop>(id + 0x30, 9.0f, comp => comp.NumTowersDone >= 4, "Towers 2/tethers 4");
            ComponentCondition<P1ProgramLoop>(id + 0x40, 9.0f, comp => comp.NumTowersDone >= 6, "Towers 3/tethers 1");
            ComponentCondition<P1ProgramLoop>(id + 0x50, 9.0f, comp => comp.NumTowersDone >= 8, "Towers 4/tethers 2")
                .DeactivateOnExit<P1ProgramLoop>();
        }

        private void P1Pantokrator(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Pantokrator, delay, 5, true);
            ComponentCondition<P1Pantokrator>(id + 0x10, 12.1f, comp => comp.NumSpreadsDone >= 2, "Spread 1/stack 3")
                .ActivateOnEnter<P1Pantokrator>()
                .ActivateOnEnter<P1BallisticImpact>()
                .ActivateOnEnter<P1FlameThrower>();
            ComponentCondition<P1Pantokrator>(id + 0x20, 6.0f, comp => comp.NumSpreadsDone >= 4, "Spread 2/stack 4");
            ComponentCondition<P1Pantokrator>(id + 0x30, 6.0f, comp => comp.NumSpreadsDone >= 6, "Spread 3/stack 1");
            ComponentCondition<P1Pantokrator>(id + 0x40, 6.0f, comp => comp.NumSpreadsDone >= 8, "Spread 4/stack 2")
                .DeactivateOnExit<P1Pantokrator>();
            ComponentCondition<P1FlameThrower>(id + 0x50, 2.1f, comp => comp.Casters.Count == 0, "Last flamethrower")
                .DeactivateOnExit<P1BallisticImpact>()
                .DeactivateOnExit<P1FlameThrower>();
        }

        private void P1WaveCannons(uint id, float delay)
        {
            ComponentCondition<P1DiffuseWaveCannonKyrios>(id, delay, comp => comp.NumCasts > 0, "Tankbusters (1) + spreads (1)")
                .ActivateOnEnter<P1DiffuseWaveCannonKyrios>()
                .ActivateOnEnter<P1WaveCannonKyrios>();
            // +2.1s: 2nd tankbuster (diffuse) hit
            // +3.9s: 2nd set of icons
            // +4.1s: 3rd tankbuster hit
            // +6.2s: 4th tankbuster hit
            ComponentCondition<P1DiffuseWaveCannonKyrios>(id + 0x10, 8.3f, comp => comp.NumCasts >= 10, "Tankbusters resolve (5)")
                .DeactivateOnExit<P1DiffuseWaveCannonKyrios>();
            ComponentCondition<P1WaveCannonKyrios>(id + 0x20, 0.7f, comp => comp.CurrentBaits.Count == 0, "Spreads (2)")
                .DeactivateOnExit<P1WaveCannonKyrios>();
        }

        private void P2FirewallSolarRay(uint id, float delay)
        {
            ActorTargetable(id, _module.BossP2M, true, delay, "M/F appear");
            ActorCast(id + 0x10, _module.BossP2M, AID.FirewallM, 1.2f, 3);
            ActorCast(id + 0x20, _module.BossP2M, AID.SolarRayM, 3.2f, 5, false, "Tankbusters")
                .ActivateOnEnter<SolarRayM>()
                .ActivateOnEnter<SolarRayF>()
                .DeactivateOnExit<SolarRayM>()
                .DeactivateOnExit<SolarRayF>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P2PartySynergy(uint id, float delay)
        {
            ActorCast(id, _module.BossP2M, AID.PartySynergyM, delay, 3);
            ActorTargetable(id + 0x10, _module.BossP2M, false, 3.1f, "M/F disappear")
                .ActivateOnEnter<P2PartySynergy>()
                .SetHint(StateMachine.StateHint.DowntimeStart);

            ComponentCondition<P2PartySynergyDoubleAOEs>(id + 0x20, 2.2f, comp => comp.AOEs.Count > 0)
                .ActivateOnEnter<P2PartySynergyDoubleAOEs>()
                .ActivateOnEnter<P2PartySynergyOpticalLaser>();
            ComponentCondition<P2PartySynergyDoubleAOEs>(id + 0x21, 5.1f, comp => comp.NumCasts > 0, "Double AOEs")
                .DeactivateOnExit<P2PartySynergyDoubleAOEs>();

            ComponentCondition<P2PartySynergyOptimizedFire>(id + 0x30, 6.4f, comp => !comp.Active, "Spreads")
                .ActivateOnEnter<P2PartySynergyOptimizedFire>()
                .ExecOnEnter<P2PartySynergyOpticalLaser>(comp => comp.Show(_module))
                .ExecOnEnter<P2PartySynergy>(comp => comp.EnableDistanceHints = true)
                .ActivateOnEnter<P2PartySynergyEfficientBladework>() // PATEs happen 0.8s after double aoes
                .DeactivateOnExit<P2PartySynergyOptimizedFire>();
            ComponentCondition<P2PartySynergyOpticalLaser>(id + 0x31, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P2PartySynergyOpticalLaser>()
                .ExecOnExit<P2PartySynergy>(comp => comp.EnableDistanceHints = false);

            ComponentCondition<P2PartySynergyDischarger>(id + 0x40, 6.9f, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2PartySynergyDischarger>()
                .ActivateOnEnter<P2PartySynergySpotlight>() // TODO: reconsider (icons appear ~0.6s after laser end, but do we even care about this?)
                .DeactivateOnExit<P2PartySynergyDischarger>();
            ComponentCondition<P2PartySynergyEfficientBladework>(id + 0x41, 4.4f, comp => comp.NumCasts > 0, "Stacks")
                .ExecOnEnter<P2PartySynergy>(comp => comp.EnableDistanceHints = true)
                .DeactivateOnExit<P2PartySynergySpotlight>() // TODO: reconsider (happens right before aoes, but do we even care?)
                .DeactivateOnExit<P2PartySynergyEfficientBladework>()
                .DeactivateOnExit<P2PartySynergy>();

            ActorTargetable(id + 0x50, _module.BossP2M, true, 3.0f, "M/F reappear")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void P2LimitlessSynergy(uint id, float delay)
        {
            ActorCast(id, _module.BossP2F, AID.SyntheticShield, delay, 1, true);
            ActorCast(id + 0x10, _module.BossP2M, AID.LimitlessSynergyF, 5.3f, 5, true, "Remove debuffs");
            ActorCastStart(id + 0x20, _module.BossP2M, AID.LaserShower, 5.0f, false, "F invincible");
            ComponentCondition<P2OptimizedBladedance>(id + 0x30, 8.5f, comp => comp.NumCasts > 0, "Baited rect + Tankbusters")
                .ActivateOnEnter<P2OptimizedBladedance>()
                .ActivateOnEnter<P2OptimizedSagittariusArrow>()
                .ActivateOnEnter<P2BeyondDefense>()
                .DeactivateOnExit<P2OptimizedBladedance>()
                .DeactivateOnExit<P2OptimizedSagittariusArrow>() // resolves right before tethers
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P2BeyondDefense>(id + 0x40, 6.0f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.Spread);
            ComponentCondition<P2BeyondDefense>(id + 0x50, 5.1f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.Stack, "Jump bait");
            // +2.8s: flares resolve, we typically don't care?..
            ComponentCondition<P2BeyondDefense>(id + 0x60, 3.2f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.None, "Flares + Stack")
                .DeactivateOnExit<P2BeyondDefense>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P2CosmoMemory>(id + 0x70, 10.2f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<P2CosmoMemory>()
                .DeactivateOnExit<P2CosmoMemory>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastEnd(id + 0x80, _module.BossP2M, 27, false, "Enrage");
        }

        private void P3Intermission(uint id, float delay)
        {
            ComponentCondition<P3SniperCannon>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3SniperCannon>();
            ComponentCondition<P3WaveRepeater>(id + 1, 0.1f, comp => comp.Sequences.Count > 0)
                .ActivateOnEnter<P3WaveRepeater>();
            ComponentCondition<P3ColossalBlow>(id + 0x10, 3.1f, comp => comp.AOEs.Count > 0)
                .ActivateOnEnter<P3ColossalBlow>();
            ComponentCondition<P3WaveRepeater>(id + 0x11, 1.9f, comp => comp.NumCasts > 0, "Ring 1");
            ComponentCondition<P3ColossalBlow>(id + 0x12, 1.1f, comp => comp.AOEs.Count > 3)
                .ActivateOnEnter<P3IntermissionVoidzone>(); // voidzone appears ~1s after first ring
            ComponentCondition<P3WaveRepeater>(id + 0x13, 1.0f, comp => comp.NumCasts > 1, "Ring 2");
            ComponentCondition<P3WaveRepeater>(id + 0x20, 1.1f, comp => comp.Sequences.Count > 1);
            ComponentCondition<P3WaveRepeater>(id + 0x21, 1.0f, comp => comp.NumCasts > 2, "Ring 3");
            ComponentCondition<P3WaveRepeater>(id + 0x22, 2.1f, comp => comp.NumCasts > 3, "Ring 4");
            ComponentCondition<P3WaveRepeater>(id + 0x30, 1.9f, comp => comp.NumCasts > 4, "Ring 5");
            ComponentCondition<P3WaveRepeater>(id + 0x31, 2.1f, comp => comp.NumCasts > 5, "Ring 6");
            ComponentCondition<P3ColossalBlow>(id + 0x40, 1.8f, comp => comp.NumCasts > 0, "Arms 1");
            ComponentCondition<P3WaveRepeater>(id + 0x41, 0.3f, comp => comp.NumCasts > 6, "Ring 7");
            ComponentCondition<P3SniperCannon>(id + 0x50, 1.7f, comp => !comp.Active, "Stack/spread")
                .DeactivateOnExit<P3SniperCannon>();
            ComponentCondition<P3WaveRepeater>(id + 0x51, 0.4f, comp => comp.NumCasts > 7)
                .DeactivateOnExit<P3WaveRepeater>();
            ComponentCondition<P3ColossalBlow>(id + 0x52, 0.2f, comp => comp.NumCasts > 3, "Arms 2")
                .DeactivateOnExit<P3ColossalBlow>();

            ActorTargetable(id + 0x100, _module.BossP3, true, 3.5f, "Boss reappears")
                .DeactivateOnExit<P3IntermissionVoidzone>(); // voidzone disappears ~1.6s before boss appears
        }

        private void P3HelloWorld(uint id, float delay)
        {
            ActorCast(id, _module.BossP3, AID.HelloWorld, delay, 5, true, "Hello World start + Raidwide")
                .ActivateOnEnter<P3HelloWorld>()
                .SetHint(StateMachine.StateHint.Raidwide);
            // +3.0s: initial smells -> bugs

            ActorCast(id + 0x10, _module.BossP3, AID.LatentDefect, 14.2f, 9, true); // ~0.1s before this cast ends first tethers are activated
            ComponentCondition<P3HelloWorld>(id + 0x12, 1, comp => comp.NumCasts > 0, "Towers 1");
            ComponentCondition<P3HelloWorld>(id + 0x13, 6, comp => comp.NumRotExplodes > 0);
            // +3.0s: tether break deadline

            ActorCast(id + 0x20, _module.BossP3, AID.LatentDefect, 5.1f, 9, true);
            ComponentCondition<P3HelloWorld>(id + 0x22, 1, comp => comp.NumCasts > 4, "Towers 2");
            //ComponentCondition<P3HelloWorld>(id + 0x23, 6, comp => comp.NumRotExplodes > 4, "Rots 2"); // note: rot 2+ explosion depends on how fast people pick it up

            ActorCast(id + 0x30, _module.BossP3, AID.LatentDefect, 11.1f, 9, true);
            ComponentCondition<P3HelloWorld>(id + 0x32, 1, comp => comp.NumCasts > 8, "Towers 3");
            //ComponentCondition<P3HelloWorld>(id + 0x33, 6, comp => comp.NumRotExplodes > 8, "Rots 3");

            ActorCast(id + 0x40, _module.BossP3, AID.LatentDefect, 11.1f, 9, true);
            ComponentCondition<P3HelloWorld>(id + 0x42, 1, comp => comp.NumCasts > 12, "Towers 4");
            //ComponentCondition<P3HelloWorld>(id + 0x43, 6, comp => comp.NumRotExplodes > 12, "Rots 4");

            ActorCast(id + 0x50, _module.BossP3, AID.CriticalError, 13.1f, 8, true, "Hello World resolve + Raidwide")
                .DeactivateOnExit<P3HelloWorld>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P3OversampledWaveCannon(uint id, float delay)
        {
            ActorCastMulti(id, _module.BossP3, new[] { AID.OversampledWaveCannonR, AID.OversampledWaveCannonL }, delay, 10, true)
                .ActivateOnEnter<P3OversampledWaveCannon>()
                .ActivateOnEnter<P3OversampledWaveCannonSpread>()
                .DeactivateOnExit<P3OversampledWaveCannon>();
            ComponentCondition<P3OversampledWaveCannonSpread>(id + 2, 0.1f, comp => !comp.Active, "Monitors")
                .DeactivateOnExit<P3OversampledWaveCannonSpread>();
        }

        private void P4WaveCannon(uint id, float delay)
        {
            ActorTargetable(id, _module.BossP3, true, delay, "Boss reappear");
            ActorCast(id + 0x10, _module.BossP3, AID.P4WaveCannonVisualStart, 9.3f, 5, true)
                .ActivateOnEnter<P4WaveCannonProtean>()
                .ActivateOnEnter<P4WaveCannonStack>(); // ~2.5s into cast: targets for stacks 1
            // +0.1s: baits
            ComponentCondition<P4WaveCannonProtean>(id + 0x20, 0.6f, comp => comp.NumCasts > 0, "Proteans 1");
            ComponentCondition<P4WaveCannonProteanAOE>(id + 0x30, 4.7f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P3WaveRepeater>() // first cast starts ~2.4s after baits
                .ActivateOnEnter<P4WaveCannonProteanAOE>()
                .ExecOnEnter<P4WaveCannonStack>(comp => comp.Imminent = true)
                .DeactivateOnExit<P4WaveCannonProteanAOE>();
            ComponentCondition<P4WaveCannonStack>(id + 0x40, 0.2f, comp => !comp.Active, "Stacks 1");
            // +2.1s: targets for stacks 2
            ComponentCondition<P3WaveRepeater>(id + 0x50, 2.4f, comp => comp.NumCasts > 0)
                .ExecOnEnter<P4WaveCannonProtean>(comp => comp.Show(Module));
            ComponentCondition<P3WaveRepeater>(id + 0x51, 2.1f, comp => comp.NumCasts > 1);
            ComponentCondition<P4WaveCannonProtean>(id + 0x52, 0.7f, comp => comp.NumCasts > 0, "Proteans 2");
            ComponentCondition<P3WaveRepeater>(id + 0x53, 1.4f, comp => comp.NumCasts > 2)
                .ActivateOnEnter<P4WaveCannonProteanAOE>()
                .ExecOnEnter<P4WaveCannonStack>(comp => comp.Imminent = true);
            ComponentCondition<P3WaveRepeater>(id + 0x54, 2.1f, comp => comp.NumCasts > 3);
            ComponentCondition<P4WaveCannonProteanAOE>(id + 0x55, 1.2f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P4WaveCannonProteanAOE>();
            ComponentCondition<P4WaveCannonStack>(id + 0x56, 0.2f, comp => !comp.Active, "Stack 2");
            // +2.2s: targets for stacks 3
            ComponentCondition<P4WaveCannonProtean>(id + 0x60, 5.3f, comp => comp.NumCasts > 0, "Proteans 3")
                .ExecOnEnter<P4WaveCannonProtean>(comp => comp.Show(Module))
                .DeactivateOnExit<P4WaveCannonProtean>();
            ComponentCondition<P3WaveRepeater>(id + 0x61, 4.5f, comp => comp.NumCasts > 4)
                .ActivateOnEnter<P4WaveCannonProteanAOE>()
                .ExecOnEnter<P4WaveCannonStack>(comp => comp.Imminent = true);
            ComponentCondition<P4WaveCannonProteanAOE>(id + 0x62, 0.3f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P4WaveCannonProteanAOE>();
            ComponentCondition<P4WaveCannonStack>(id + 0x63, 0.2f, comp => !comp.Active, "Stack 3")
                .DeactivateOnExit<P4WaveCannonStack>();
            ComponentCondition<P3WaveRepeater>(id + 0x64, 1.7f, comp => comp.NumCasts > 5);
            ComponentCondition<P3WaveRepeater>(id + 0x65, 2.1f, comp => comp.NumCasts > 6);
            ComponentCondition<P3WaveRepeater>(id + 0x66, 2.1f, comp => comp.NumCasts > 7, "Ring 8")
                .DeactivateOnExit<P3WaveRepeater>();
        }

        private void P4BlueScreen(uint id, float delay)
        {
            ActorCast(id, _module.BossP3, AID.BlueScreen, delay, 8, true);
            ComponentCondition<P4BlueScreen>(id + 2, 1, comp => comp.NumCasts > 0, "Enrage", 100)
                .ActivateOnEnter<P4BlueScreen>()
                .DeactivateOnExit<P4BlueScreen>();
        }

        private void P5SolarRay(uint id, float delay, bool afterModelChange)
        {
            ActorCast(id, _module.BossP5, afterModelChange ? AID.P5SolarRayF : AID.P5SolarRayM, delay, 5, true, "Tankbuster 1")
                .ActivateOnEnter<P5SolarRay>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P5SolarRay>(id + 2, 3.2f, comp => comp.NumCasts > 1, "Tankbuster 2")
                .DeactivateOnExit<P5SolarRay>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P5RunMiDelta(uint id, float delay)
        {
            ActorCast(id, _module.BossP5, AID.RunMiDeltaVersion, delay, 5, true, "Trio 1 raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x10, _module.BossP5, false, 3.1f, "Boss disappears")
                .ActivateOnEnter<P5DeltaOpticalLaser>()
                .ActivateOnEnter<P5Delta>()
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P5DeltaOpticalLaser>(id + 0x11, 0.1f, comp => comp.Source != null);
            ComponentCondition<P5Delta>(id + 0x20, 8.1f, comp => comp.NumPunchesSpawned > 0, "Fists spawn");
            ComponentCondition<P5Delta>(id + 0x30, 7.2f, comp => comp.ArmRotations.Any(r => r != default));
            ComponentCondition<P5Delta>(id + 0x40, 2.6f, comp => comp.TethersActive, "Tethers active"); // first tether should be broken immediately (inner blue)

            ComponentCondition<P5DeltaOpticalLaser>(id + 0x50, 2.1f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P5DeltaExplosion>()
                .DeactivateOnExit<P5DeltaOpticalLaser>();
            ComponentCondition<P5DeltaExplosion>(id + 0x51, 0.1f, comp => comp.Casters.Count > 0, "Puddles bait");

            ActorCastStart(id + 0x60, _module.BossP5, AID.BeyondDefense, 0.3f, true) // note: monitors status + cast start happen right before, but we don't care yet...
                .ActivateOnEnter<P5DeltaHyperPulse>();
            // second tether break happens somewhere here (outer blue)
            ComponentCondition<P5DeltaExplosion>(id + 0x61, 2.7f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2BeyondDefense>()
                .DeactivateOnExit<P5DeltaExplosion>();
            ActorCastEnd(id + 0x62, _module.BossP5, 2.2f, true);
            ComponentCondition<P2BeyondDefense>(id + 0x63, 0.3f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.Stack, "Bait rotates & jump");

            ComponentCondition<P5DeltaOversampledWaveCannon>(id + 0x70, 4.8f, comp => !comp.Active, "Monitors")
                .ActivateOnEnter<P5DeltaOversampledWaveCannon>()
                .DeactivateOnExit<P5DeltaOversampledWaveCannon>();
            ComponentCondition<P2BeyondDefense>(id + 0x71, 0.5f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.None)
                .DeactivateOnExit<P2BeyondDefense>();
            ComponentCondition<P5DeltaHyperPulse>(id + 0x72, 0.1f, comp => comp.NumCasts >= 36)
                .DeactivateOnExit<P5DeltaHyperPulse>();

            ComponentCondition<P5DeltaSwivelCannon>(id + 0x80, 2.4f, comp => comp.AOE != null)
                .ActivateOnEnter<P5DeltaSwivelCannon>();
            // third tether break happens somewhere here (inner green)
            ComponentCondition<P5DeltaSwivelCannon>(id + 0x82, 10, comp => comp.AOE == null, "Cleave")
                .ActivateOnEnter<P5NearDistantWorld>()
                .DeactivateOnExit<P5DeltaSwivelCannon>();
            ComponentCondition<P5NearDistantWorld>(id + 0x83, 0.7f, comp => comp.NumNearJumpsDone > 0, "Near/far 1");
            ComponentCondition<P5NearDistantWorld>(id + 0x84, 1.0f, comp => comp.NumNearJumpsDone > 1, "Near/far 2");
            ComponentCondition<P5NearDistantWorld>(id + 0x85, 1.0f, comp => comp.NumNearJumpsDone > 2, "Near/far 3")
                .DeactivateOnExit<P5NearDistantWorld>();
            ActorTargetable(id + 0x90, _module.BossP5, true, 2.3f, "Boss reappears")
                .DeactivateOnExit<P5Delta>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            // fourth tether break happens somewhere here, after mechanic ends
        }

        private void P5RunMiSigma(uint id, float delay)
        {
            ActorCast(id, _module.BossP5, AID.RunMiSigmaVersion, delay, 5, true, "Trio 2 raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x10, _module.BossP5, false, 3.0f, "Boss disappears")
                .ActivateOnEnter<P5Sigma>() // icons/tethers/statuses appear right as boss disappears
                .SetHint(StateMachine.StateHint.DowntimeStart);

            ComponentCondition<P5SigmaWaveCannon>(id + 0x20, 10.3f, comp => comp.CurrentBaits.Count > 0)
                .ActivateOnEnter<P5SigmaWaveCannon>();
            ComponentCondition<P5SigmaHyperPulse>(id + 0x22, 8.9f, comp => comp.NumCasts > 0, "Proteans")
                .ActivateOnEnter<P5SigmaHyperPulse>()
                .DeactivateOnExit<P5SigmaHyperPulse>();
            ComponentCondition<P5SigmaWaveCannon>(id + 0x23, 0.2f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P5SigmaWaveCannon>();

            ComponentCondition<P5SigmaTowers>(id + 0x40, 2.8f, comp => comp.Towers.Count > 0)
                .ActivateOnEnter<P5SigmaTowers>();
            ComponentCondition<P2PartySynergyDischarger>(id + 0x41, 5.9f, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2PartySynergyDischarger>()
                .DeactivateOnExit<P2PartySynergyDischarger>();
            ComponentCondition<P5SigmaTowers>(id + 0x42, 3.9f, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<P5SigmaTowers>()
                .DeactivateOnExit<P5Sigma>();

            ComponentCondition<P5SigmaRearLasers>(id + 0x100, 4.3f, comp => comp.Active)
                .ActivateOnEnter<P5SigmaDoubleAOEs>()
                .ActivateOnEnter<P5SigmaRearLasers>();
            ComponentCondition<P5SigmaRearLasers>(id + 0x110, 10.1f, comp => comp.NumCasts > 0, "Lasers start"); // note: cast starts 3s before
            ComponentCondition<P5SigmaDoubleAOEs>(id + 0x120, 3.2f, comp => comp.NumCasts > 0, "Cross/sides") // note: cast starts ~1.5s before
                .ExecOnEnter<P5SigmaDoubleAOEs>(comp => comp.Show = true)
                .DeactivateOnExit<P5SigmaDoubleAOEs>();
            ComponentCondition<P5SigmaRearLasers>(id + 0x130, 4.4f, comp => comp.NumCasts >= 14, "Lasers end")
                .ActivateOnEnter<P5SigmaHyperPulse>() // note: this starts way earlier, but there's no point showing hints before cross/sides are done
                .ActivateOnEnter<P5SigmaNearDistantWorld>()
                .DeactivateOnExit<P5SigmaRearLasers>();
            ComponentCondition<P5SigmaNearDistantWorld>(id + 0x140, 2.3f, comp => comp.NumNearJumpsDone > 0, "Near/far 1");
            ComponentCondition<P5SigmaNearDistantWorld>(id + 0x142, 1.0f, comp => comp.NumNearJumpsDone > 1, "Near/far 2");
            ComponentCondition<P5SigmaNearDistantWorld>(id + 0x143, 1.0f, comp => comp.NumNearJumpsDone > 2, "Near/far 3")
                .DeactivateOnExit<P5SigmaNearDistantWorld>()
                .DeactivateOnExit<P5SigmaHyperPulse>(); // note: this resolves soon after first near/far

            ActorTargetable(id + 0x150, _module.BossP5, true, 2.5f, "Boss reappears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void P5RunMiOmega(uint id, float delay)
        {
            ActorCast(id, _module.BossP5, AID.RunMiOmegaVersion, delay, 5, true, "Trio 3 raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P5OmegaNearDistantWorld>(id + 0x10, 3.1f, comp => comp.HaveDebuffs)
                .ActivateOnEnter<P5OmegaNearDistantWorld>();
            ComponentCondition<P5OmegaDoubleAOEs>(id + 0x20, 2, comp => comp.AOEs.Count > 0)
                .ActivateOnEnter<P5OmegaDoubleAOEs>()
                .ActivateOnEnter<P5OmegaDiffuseWaveCannon>();
            ComponentCondition<P5OmegaDoubleAOEs>(id + 0x21, 4, comp => comp.AOEs.Count > 3); // each set is 2 or 3 aoes
            ComponentCondition<P5OmegaDoubleAOEs>(id + 0x30, 9.1f, comp => comp.NumCasts > 0, "AOEs 1"); // diffuse wave cannon 1 happens right after that
            ComponentCondition<P5OmegaDoubleAOEs>(id + 0x40, 4, comp => comp.NumCasts > 3, "AOEs 2")
                .DeactivateOnExit<P5OmegaDoubleAOEs>()
                .DeactivateOnExit<P5OmegaDiffuseWaveCannon>(); // this resolves right after
            ActorTargetable(id + 0x50, _module.BossP5, false, 3.1f, "Boss disappears")
                .SetHint(StateMachine.StateHint.DowntimeStart);

            ComponentCondition<P5OmegaOversampledWaveCannon>(id + 0x100, 0.2f, comp => comp.IsActive)
                .ExecOnEnter<P5OmegaNearDistantWorld>(comp => comp.ShowFirst(_module))
                .ActivateOnEnter<P5OmegaOversampledWaveCannon>();
            ComponentCondition<P5OmegaNearDistantWorld>(id + 0x110, 9.6f, comp => comp.NumNearJumpsDone > 0, "Near/far 1");
            ComponentCondition<P5OmegaNearDistantWorld>(id + 0x112, 1.0f, comp => comp.NumNearJumpsDone > 1, "Near/far 2");
            ComponentCondition<P5OmegaNearDistantWorld>(id + 0x113, 1.0f, comp => comp.NumNearJumpsDone > 2, "Near/far 3")
                .DeactivateOnExit<P5OmegaOversampledWaveCannon>(); // note: resolves between first and second jumps

            ComponentCondition<P5OmegaBlaster>(id + 0x200, 2.1f, comp => comp.CurrentBaits.Count > 0)
                .ExecOnEnter<P5OmegaNearDistantWorld>(comp => comp.ShowSecond(_module))
                .ActivateOnEnter<P5OmegaBlaster>();
            ComponentCondition<P5OmegaBlaster>(id + 0x210, 12.1f, comp => comp.NumCasts > 0, "???")
                .DeactivateOnExit<P5OmegaBlaster>();
            // TODO: near/distant worlds...
        }
    }
}
