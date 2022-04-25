using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Madhunt {
    [CustomEntity("Madhunt/HiderBerryTokenFollower")]
    public class HiderBerryTokenFollower : Entity {
        public static ParticleType P_Glow;
        public HiderBerryToken Parent;
        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private float wobble;
        public Follower Follower;
        private Vector2 start;
        private float collectTimer;
        private bool collected;

        public HiderBerryTokenFollower(Vector2 position, HiderBerryToken parent, Player player) {
            this.Position = this.start = position;
            this.Parent = parent;
            this.Depth = -100;
            this.Add(new MirrorReflection());
            this.Add(this.Follower = new Follower(onLoseLeader: this.OnLoseLeader));
            this.Follower.FollowDelay = 0.3f;
            player.Leader.GainFollower(this.Follower);
        }
        
        public override void Added(Scene scene) {
            base.Added(scene);
            // TODO make custom sprite
            if (this.collected) {
                this.sprite = GFX.SpriteBank.Create("ghostberry");
            } else {
                this.sprite = GFX.SpriteBank.Create("strawberry");
            }

            this.Add(this.sprite);
            this.sprite.OnFrameChange = this.OnAnimate;
            this.Add(this.wiggler = Wiggler.Create(0.4f, 4f, v => this.sprite.Scale = Vector2.One * (float) (1.0 + v * 0.35)));
            this.wiggler.Start();
            this.Add(this.bloom = new BloomPoint(1f, 12f));
            this.Add(this.light = new VertexLight(Color.White, 1f, 16, 24));
            this.Add(this.lightTween = this.light.CreatePulseTween());
            if ((scene as Level).Session.BloomBaseAdd <= 0.1) return;
            this.bloom.Alpha *= 0.5f;
        }
        
        private void OnAnimate(string id) {
            if (this.sprite.CurrentAnimationFrame != 35)
                return;
            this.lightTween.Start();
            if (!this.collected && (this.CollideCheck<FakeWall>() || this.CollideCheck<Solid>())) {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.1f);
            } else {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.2f);
            }
        }
        
        private void OnLoseLeader() {
            if(this.collected)
                return;
            Alarm.Set(this, 0.15f, () => {
                Vector2 vector = (this.start - this.Position).SafeNormalize();
                float val = Vector2.Distance(this.Position, this.start);
                float num = Calc.ClampedMap(val, 16f, 120f, 16f, 96f);
                SimpleCurve curve = new SimpleCurve(this.Position, this.start, this.start + vector * 16f + vector.Perpendicular() * num * Calc.Random.Choose(1, -1));
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(val / 100f, 0.4f), true);
                tween.OnUpdate = f => this.Position = curve.GetPoint(f.Eased);
                tween.OnComplete = LoseLeaderTweenOnComplete;
                this.Add(tween);
            });
        }

        private void LoseLeaderTweenOnComplete(Tween f) {
            this.Depth = 0;
            this.RemoveSelf();
            this.Parent.OnLoseLeaderComplete();
        }

        public void OnCollect() {
            if (this.collected)
                return;
            int collectIndex = 0;
            this.collected = true;
            if (this.Follower.Leader != null) {
                Player entity = this.Follower.Leader.Entity as Player;
                collectIndex = entity.StrawberryCollectIndex;
                ++entity.StrawberryCollectIndex;
                entity.StrawberryCollectResetTimer = 2.5f;
                this.Follower.Leader.LoseFollower(this.Follower);
            }
            Parent.OnCollect();
            this.Add(new Coroutine(this.CollectRoutine(collectIndex)));
        }
        
        private IEnumerator CollectRoutine(int collectIndex) {
            Scene scene = this.Scene;
            this.Tag = (int) Tags.TransitionUpdate;
            this.Depth = -2000010;
            Audio.Play("event:/game/general/strawberry_get", this.Position, "colour", 0, "count", collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            this.sprite.Play("collect");
            while (this.sprite.Animating)
                yield return null;
            // TODO replace StrawberryPoints with an animation
            this.RemoveSelf();
        }
        
        public override void Update() {
            if (!this.collected) {
                this.wobble += Engine.DeltaTime * 4f;
                this.sprite.Y = this.bloom.Y = this.light.Y = (float) Math.Sin(this.wobble) * 2f;
                int followIndex = this.Follower.FollowIndex;
                if (this.Follower.Leader != null && this.Follower.DelayTimer <= 0.0 && this.IsFirstToken()) {
                    Player entity = this.Follower.Leader.Entity as Player;
                    bool flag = false;
                    if (entity != null && entity.Scene != null && !entity.StrawberriesBlocked) {
                        if (entity.OnSafeGround)
                            flag = true;
                    }

                    if (flag) {
                        this.collectTimer += Engine.DeltaTime;
                        if (this.collectTimer > 0.15)
                            this.OnCollect();
                    } else {
                        this.collectTimer = Math.Min(this.collectTimer, 0.0f);
                    }
                } else {
                    if (followIndex > 0)
                        this.collectTimer = -0.15f;
                }
            }
            base.Update();
            if (this.Follower.Leader == null || !this.Scene.OnInterval(0.08f))
                return;
            ParticleType type = HiderBerryTokenFollower.P_Glow;
            this.SceneAs<Level>().ParticlesFG.Emit(type, this.Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }
    
        public bool IsFirstToken() {
            for (int index = this.Follower.FollowIndex - 1; index >= 0; --index) {
                Entity entity = this.Follower.Leader.Followers[index].Entity;
                if (entity is HiderBerryTokenFollower) return false;
            }
            return true;
        }
    }
}
