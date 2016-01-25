using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2
{
    public class ResourceCell
    {
        // amount in percent
        public float Iron;
        public float Copper;
        public float Aluminium;
        public float Lithium;
        public float Titanium;
        public float Nickel;
        public float Silver;
        public float Tungsten;
        public float Platinum;
        public float Gold;
        public float Lead;
        public float Uranium;

        public void Randomize(Random rnd)
        {
            //Random rnd = new Random();
            Iron = rnd.Next(100);
            Copper = rnd.Next(100);
            Aluminium = rnd.Next(100);
            Lithium = rnd.Next(100);
            Titanium = rnd.Next(100);
            Nickel = rnd.Next(100);
            Silver = rnd.Next(100);
            Tungsten = rnd.Next(100);
            Platinum = rnd.Next(100);
            Gold = rnd.Next(100);
            Lead = rnd.Next(100);
            Uranium = rnd.Next(100);
            Normalize();
        }

        public void Normalize()
        {
            float val = Iron + Copper + Aluminium + Lithium + Titanium + Nickel + Silver + Tungsten + Platinum + Gold + Lead + Uranium;

            Iron = Iron / val * 100;
            Copper = Copper / val * 100;
            Aluminium = Aluminium / val * 100;
            Lithium = Lithium / val * 100;
            Titanium = Titanium / val * 100;
            Nickel = Nickel / val * 100;
            Silver = Silver / val * 100;
            Tungsten = Tungsten / val * 100;
            Platinum = Platinum / val * 100;
            Gold = Gold / val * 100;
            Lead = Lead / val * 100;
            Uranium = Uranium / val * 100;
        }



        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("Iron: "); sb.Append((int)(Iron * 1)); sb.AppendLine();
            sb.Append("Copper: "); sb.Append((int)(Copper * 1)); sb.AppendLine();
            sb.Append("Aluminium: "); sb.Append((int)(Aluminium * 1)); sb.AppendLine();
            sb.Append("Lithium: "); sb.Append((int)(Lithium * 1)); sb.AppendLine();
            sb.Append("Titanium: "); sb.Append((int)(Titanium * 1)); sb.AppendLine();
            sb.Append("Nickel: "); sb.Append((int)(Nickel * 1)); sb.AppendLine();
            sb.Append("Silver: "); sb.Append((int)(Silver * 1)); sb.AppendLine();
            sb.Append("Tungsten: "); sb.Append((int)(Tungsten * 1)); sb.AppendLine();
            sb.Append("Platinum: "); sb.Append((int)(Platinum * 1)); sb.AppendLine();
            sb.Append("Gold: "); sb.Append((int)(Gold * 1)); sb.AppendLine();
            sb.Append("Lead: "); sb.Append((int)(Lead * 1)); sb.AppendLine();
            sb.Append("Uranium: "); sb.Append((int)(Uranium * 1)); sb.AppendLine();

            return sb.ToString();
        }

    }
}

/*
         
Iron
Copper
Aluminium
Lithium
Titanium
Nickel
Silver
Tungsten
Platinum
Gold
Lead
Uranium
          
 * */