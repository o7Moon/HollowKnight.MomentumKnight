using GlobalEnums;
using Modding;
using System.Reflection;
using On;
using UnityEngine;
using System.Collections.Generic;
using Modding.Menu;

namespace MomentumKnight {
    public class MomentumKnight : Mod, ITogglableMod, IGlobalSettings<GlobalModSettings>, IMenuMod
    {
        public static MomentumKnight LoadedInstance { get; set; }
        
        public bool ToggleButtonInsideMenu { get; set;}
        
        public static GlobalModSettings SettingsInstance { get; set; } = new GlobalModSettings();
        
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        public void OnLoadGlobal(GlobalModSettings settings){
            MomentumKnight.SettingsInstance = settings;
        }
        
        public GlobalModSettings OnSaveGlobal(){
            return MomentumKnight.SettingsInstance;
        }
        
        public string[] stringRange(int len){
            string[] r = new string[len];
            for(int i = 1; i <= len; i++){
                r[i-1] = i.ToString();
            }
            return r;
        }
        
        public override void Initialize()
        {
            if (MomentumKnight.LoadedInstance != null) return;
            MomentumKnight.LoadedInstance = this;
            On.HeroController.Move += heroControllerMoveHook;
        }
        
        public void Unload()
        {
            On.HeroController.Move += heroControllerMoveHook;
            MomentumKnight.LoadedInstance = null;
        }
        public void heroControllerMoveHook(On.HeroController.orig_Move orig, HeroController self, float move_direction){
            Vector2 r = ReflectionHelper.GetField<HeroController,Rigidbody2D>(self,"rb2d").velocity;
            if (self.cState.onGround)
            {
                ActorStates newState = ActorStates.grounded;
                if (Mathf.Abs(self.move_input) > Mathf.Epsilon)
                {
                    newState = ActorStates.running;
                }
                else
                {
                    newState = ActorStates.idle;
                }
                if (newState != self.hero_state)
                {
                    self.prev_hero_state = self.hero_state;
                    self.hero_state = newState;
                    ReflectionHelper.GetField<HeroController,HeroAnimationController>(self,"animCtrl").UpdateState(newState);
                }
            }
            if (self.acceptingInput && !self.cState.wallSliding)
            {
                if (self.cState.inWalkZone)
                {
                    r = new Vector2(r.x + move_direction * MomentumKnight.SettingsInstance.walkAccel*0.05f, r.y);
                    r.x *= 1-(MomentumKnight.SettingsInstance.walkFriction*0.01f);
                }
                else if (self.inAcid)
                {
                    r = new Vector2(move_direction * self.UNDERWATER_SPEED, r.y);
                }
                else if (self.playerData.GetBool("equippedCharm_37") && self.cState.onGround && self.playerData.GetBool("equippedCharm_31"))
                {
                    r = new Vector2(r.x + move_direction * MomentumKnight.SettingsInstance.runAccel*1.37f*0.05f, r.y);
                    r.x *= 1-(MomentumKnight.SettingsInstance.runFriction*0.01f);
                }
                else if (self.playerData.GetBool("equippedCharm_37") && self.cState.onGround)
                {
                    r = new Vector2(r.x + move_direction * MomentumKnight.SettingsInstance.runAccel*1.2048192771f*0.05f, r.y);
                    r.x *= 1-(MomentumKnight.SettingsInstance.runFriction*0.01f);
                } else { 
                    r = new Vector2(r.x + move_direction * MomentumKnight.SettingsInstance.runAccel*0.05f, r.y);
                    r.x *= 1-(MomentumKnight.SettingsInstance.runFriction*0.01f);
                }
                ReflectionHelper.GetField<HeroController,Rigidbody2D>(self,"rb2d").velocity = r;
            }
            //orig(self, ReflectionHelper.GetField<HeroController,Rigidbody2D>(self,"rb2d").velocity.x*(self.cState.inWalkZone ? 0.155f : 0.115f)+move_direction*0.10f);
        }
        
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> r = new List<IMenuMod.MenuEntry> {
                new IMenuMod.MenuEntry {
                    Name = "walking acceleration",
                    Description = "1-100",
                    Values = stringRange(100),
                    Saver = opt => MomentumKnight.SettingsInstance.walkAccel = opt,
                    Loader = () => MomentumKnight.SettingsInstance.walkAccel
                },
                new IMenuMod.MenuEntry {
                    Name = "walking friction",
                    Description = "1-100% (velocity is decreased by this amount every frame)",
                    Values = stringRange(100),
                    Saver = opt => MomentumKnight.SettingsInstance.walkFriction = opt,
                    Loader = () => MomentumKnight.SettingsInstance.walkFriction
                },
                new IMenuMod.MenuEntry {
                    Name = "running acceleration",
                    Description = "1-100",
                    Values = stringRange(100),
                    Saver = opt => MomentumKnight.SettingsInstance.runAccel = opt,
                    Loader = () => MomentumKnight.SettingsInstance.runAccel
                },
                new IMenuMod.MenuEntry {
                    Name = "running friction",
                    Description = "1-100% (velocity is decreased by this amount every frame)",
                    Values = stringRange(100),
                    Saver = opt => MomentumKnight.SettingsInstance.runFriction = opt,
                    Loader = () => MomentumKnight.SettingsInstance.runFriction
                }
            };
            return r;
        }
    }
}
