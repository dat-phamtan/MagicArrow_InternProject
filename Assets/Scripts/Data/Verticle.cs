using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public enum VerticleType {NONE, DOT, TAIL, BODY, HEAD};
    public class Verticle
    {
        public float XVerticle { get; set; }
        public float YVerticle { get; set; }
        public VerticleType Type { get; set; }

        public Verticle(float xVerticle, float yVerticle, VerticleType type)
        {
            XVerticle = xVerticle;
            YVerticle = yVerticle;
            Type = type;
        }
    }
}
