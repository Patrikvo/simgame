using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Simgame2
{

    public class EntityFactory : Microsoft.Xna.Framework.GameComponent
    {
     //   private Effect modelEffect;
    //    private WorldMap worldMap;

        private LODTerrain.LODTerrain LODMap;

        private EntityFactory(Game game, LODTerrain.LODTerrain map, Matrix projectionMatrix)
        //private EntityFactory(Game game, WorldMap map, Matrix projectionMatrix)
            : base(game)
        {
            this.projectionMatrix = projectionMatrix;

       //     modelEffect = game.Content.Load<Effect>("Series4Effects");
            
            //this.worldMap = map;
            this.LODMap = map;
            this.buildings = new List<EntityBuilding>();
            this.Movers = new List<Entity>();
        }

        public static EntityFactory CreateFactory(Game game, LODTerrain.LODTerrain map, Matrix projectionMatrix)
        //public static EntityFactory CreateFactory(Game game, WorldMap map, Matrix projectionMatrix)
        {
            if (factorySingleton == null)
            {
                factorySingleton = new EntityFactory(game, map, projectionMatrix);
            }
            return factorySingleton;
        }


        public override void Initialize()
        {

            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            foreach (EntityBuilding building in buildings)
            {
                building.Update(gameTime);
            }

            foreach (Entity e in Movers)
            {
                e.Update(gameTime);
            }


            base.Update(gameTime);
        }


        public Entity CreateEnity(Entity.EntityTypes entityType, Vector3 location, bool flatten)
        {
            switch (entityType) 
            { 
                case Entity.EntityTypes.BASIC_MINE:
                    return CreateBasicMine(location, flatten);
                case Entity.EntityTypes.MELTER:
                    return CreateBasicMelter(location, flatten);
                case Entity.EntityTypes.SOLAR:
                    return CreateBasicSolarPlant(location, flatten);
                case Entity.EntityTypes.WIND_TOWER:
                    return CreateWindTower(location, flatten);
                case Entity.EntityTypes.LANDER:
                    return CreateLander(location, flatten);
                case Entity.EntityTypes.MOVER:
                    


                default:
                    return null;
            }
        }


        public Entities.MoverEntity CreateMover(Vector3 location)
        {
            Entities.MoverEntity mover = new Entities.MoverEntity(this.Game);
            mover.projectionMatrix = this.projectionMatrix;
            //int height = ((Game1)this.Game).worldMap.getCellHeightFromWorldCoor(location.X, -location.Z);
            int height = ((Game1)this.Game).LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            mover.location = new Vector3(location.X, height, location.Z);
            mover.scale = new Vector3(5.0f, 5.0f, 5.0f);
            mover.UpdateBoundingBox();
            //mover.ShowBoundingBox = true;
            this.Movers.Add(mover);
            return mover;

        }

        public Entities.MoverEntity CreateMiniMover(Vector3 location)
        {
            Entities.MoverEntity mover = new Entities.MoverEntity(this.Game);
            mover.projectionMatrix = this.projectionMatrix;
            //int height = ((Game1)this.Game).worldMap.getCellHeightFromWorldCoor(location.X, -location.Z);
            int height = ((Game1)this.Game).LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            mover.location = new Vector3(location.X, height, location.Z);
            mover.scale = new Vector3(2.0f, 2.0f, 2.0f);
            mover.UpdateBoundingBox();
          //  mover.ShowBoundingBox = true;
            this.Movers.Add(mover);
            return mover;

        }


        public Buildings.Lander CreateLander(Vector3 location, bool flatten)
        {
            Buildings.Lander lander = new Buildings.Lander(this.Game);
            lander.projectionMatrix = this.projectionMatrix;
            lander.Initialize(Buildings.Lander.StandardScale, Buildings.Lander.StandardRotation);
            int height = ((Game1)this.Game).LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            lander.location = new Vector3(location.X, height, location.Z);
            this.buildings.Add(lander);
            return lander;
        }



        public Buildings.BasicMine CreateBasicMine(Vector3 location, bool flatten)
        {
            Buildings.BasicMine mine = new Buildings.BasicMine(this.Game);
            mine.projectionMatrix = this.projectionMatrix;
         //   mine.effect = this.modelEffect;
            mine.Initialize(Buildings.BasicMine.StandardScale, Buildings.BasicMine.StandardRotation);
          //  mine.Place(worldMap, location, flatten);
            this.buildings.Add(mine);
            return mine;
        }

        public Buildings.WindTower CreateWindTower(Vector3 location, bool flatten)
        {
            Buildings.WindTower tower = new Buildings.WindTower(this.Game);
            tower.projectionMatrix = this.projectionMatrix;
            tower.Initialize(Buildings.WindTower.StandardScale, Buildings.WindTower.StandardRotation);
            this.buildings.Add(tower);
            return tower;
        }

        public Buildings.Melter CreateBasicMelter(Vector3 location, bool flatten)
        {
            Buildings.Melter melter = new Buildings.Melter(this.Game);
            melter.projectionMatrix = this.projectionMatrix;
       //     melter.effect = this.modelEffect;
            melter.Initialize(Buildings.Melter.StandardScale, Buildings.Melter.StandardRotation);
         //   melter.Place(worldMap, location, flatten);
            this.buildings.Add(melter);
            return melter;
        }

        public Buildings.SolarPlant CreateBasicSolarPlant(Vector3 location, bool flatten)
        {
            Buildings.SolarPlant solar = new Buildings.SolarPlant(this.Game);
            solar.projectionMatrix = this.projectionMatrix;
        //    solar.effect = this.modelEffect;
            solar.Initialize(Buildings.SolarPlant.StandardScale, Buildings.SolarPlant.StandardRotation);
          //  solar.Place(worldMap, location, flatten);
            this.buildings.Add(solar);
            return solar;
        }


        public void RemoveBuilding(EntityBuilding building)
        {
     //       building.RemoveBuilding(worldMap);
            this.buildings.Remove(building);
        }


        private List<EntityBuilding> buildings;
        private List<Entity> Movers;

        private static EntityFactory factorySingleton;
        private Matrix projectionMatrix;
    }
}
