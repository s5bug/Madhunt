using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Madhunt {
    [CustomEntity("Madhunt/HiderBerryToken")]
    public class HiderBerryToken : Entity {
        public enum State {
            Uncollected,
            InTransit,
            Collected,
        }
        
        public EntityID ID;
        private Level level;
        private Sprite sprite;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private State state;
        
        private static Color uncollectedColor = Calc.HexToColor("5fcde4");
        private static Color inTransitColor = Color.White;
        private static Color collectedColor = Calc.HexToColor("f141df");
        
        public HiderBerryToken(EntityData data, Vector2 offset, EntityID gid) {
            this.ID = gid;
            this.Position = data.Position + offset;
            this.Depth = -100;
            this.Collider = new Hitbox(14f, 14f, -7f, -7f);
            this.Add(new PlayerCollider(this.OnPlayer));
            this.Add(new MirrorReflection());
            if (data.Nodes == null || data.Nodes.Length == 0)
                return;
        }
        
        public override void Added(Scene scene) {
            base.Added(scene);
            this.level = scene as Level;
            // TODO make custom sprite
            this.sprite = GFX.SpriteBank.Create("strawberry");
            if (Module.Session.HiderBerryTokens.Contains(this.ID)) {
                this.state = State.Collected;
                this.sprite.Color = collectedColor;
            } else {
                this.state = State.Uncollected;
                this.sprite.Color = uncollectedColor;
            }

            this.Add(this.sprite);
            this.sprite.OnFrameChange = this.OnAnimate;
            this.Add(this.bloom = new BloomPoint(1f, 12f));
            this.Add(this.light = new VertexLight(Color.White, 1f, 16, 24));
            this.Add(this.lightTween = this.light.CreatePulseTween());
            if (this.level.Session.BloomBaseAdd <= 0.1) return;
            this.bloom.Alpha *= 0.5f;
        }
        
        private void OnAnimate(string id) {
            if (this.sprite.CurrentAnimationFrame != 35)
                return;
            this.lightTween.Start();
            if (this.state == State.Uncollected && (this.CollideCheck<FakeWall>() || this.CollideCheck<Solid>())) {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.level.Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.1f);
            } else {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.level.Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.2f);
            }
        }
        
        public void OnPlayer(Player player) {
            if (this.state == State.Uncollected) {
                Audio.Play("event:/game/general/strawberry_touch", this.Position);
                this.level.Add(new HiderBerryTokenFollower(this.Position, this, player));
                this.Depth = -1000000;
                this.state = State.InTransit;
                this.sprite.Color = inTransitColor;
                // TODO change sprite
            }
        }

        public void OnLoseLeaderComplete() {
            this.state = State.Uncollected;
            this.sprite.Color = uncollectedColor;
        }
        
        public void OnCollect() {
            Module.Session.HiderBerryTokens.Add(this.ID);
            this.Depth = 200;
            this.state = State.Collected;
            this.sprite.Color = collectedColor;
        }
    }
}
