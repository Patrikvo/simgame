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

    public class EntityFactory
    {
     
        private LODTerrain.LODTerrain LODMap;

        protected GameSession.GameSession RunningGameSession;

        private EntityFactory(GameSession.GameSession RunningGameSession, LODTerrain.LODTerrain map, Matrix projectionMatrix)
        {
            this.projectionMatrix = projectionMatrix;

            this.LODMap = map;
            this.buildings = new List<EntityBuilding>();
            this.Movers = new List<Entity>();

            this.RunningGameSession = RunningGameSession;

        }

        public static EntityFactory CreateFactory(GameSession.GameSession RunningGameSession, LODTerrain.LODTerrain map, Matrix projectionMatrix)
        {
            if (factorySingleton == null)
            {
                factorySingleton = new EntityFactory(RunningGameSession, map, projectionMatrix);
            }
            return factorySingleton;
        }


        public virtual void Initialize()
        {
        }


        public virtual void Update(GameTime gameTime)
        {
            foreach (EntityBuilding building in buildings)
            {
                building.Update(gameTime);
            }

            foreach (Entity e in Movers)
            {
                e.Update(gameTime);
            }
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
            Entities.MoverEntity mover = new Entities.MoverEntity(this.RunningGameSession);
            mover.projectionMatrix = this.projectionMatrix;
            int height = this.RunningGameSession.LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            mover.location = new Vector3(location.X, height, location.Z);
            mover.scale = new Vector3(5.0f, 5.0f, 5.0f);
            mover.UpdateBoundingBox();
            //mover.ShowBoundingBox = true;
            this.Movers.Add(mover);
            this.RunningGameSession.simulator.AddEntity(mover);
            return mover;

        }

        public Entities.MoverEntity CreateMiniMover(Vector3 location)
        {
            Entities.MoverEntity mover = new Entities.MoverEntity(this.RunningGameSession);
            mover.projectionMatrix = this.projectionMatrix;
            int height = this.RunningGameSession.LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            mover.location = new Vector3(location.X, height, location.Z);
            mover.scale = new Vector3(2.0f, 2.0f, 2.0f);
            mover.UpdateBoundingBox();
          //  mover.ShowBoundingBox = true;
            this.Movers.Add(mover);
            this.RunningGameSession.simulator.AddEntity(mover);
            return mover;

        }

        public Entities.EntityFlyer CreateFlyer(Vector3 location)
        {
            Entities.EntityFlyer flyer = new Entities.EntityFlyer(this.RunningGameSession);
            flyer.projectionMatrix = this.projectionMatrix;
            float height = this.RunningGameSession.LODMap.getCellHeightFromWorldCoor(location.X, location.Z) + flyer.MinHeight;

            flyer.location = new Vector3(location.X, height, location.Z);
            flyer.scale = new Vector3(5.0f, 5.0f, 5.0f);
            flyer.UpdateBoundingBox();
            //mover.ShowBoundingBox = true;
            this.Movers.Add(flyer);
            this.RunningGameSession.simulator.AddEntity(flyer);
            return flyer;

        }



        public Buildings.Lander CreateLander(Vector3 location, bool flatten)
        {
            Buildings.Lander lander = new Buildings.Lander(this.RunningGameSession);
            lander.projectionMatrix = this.projectionMatrix;
            lander.Initialize(Buildings.Lander.StandardScale, Buildings.Lander.StandardRotation);
            int height = this.RunningGameSession.LODMap.getCellHeightFromWorldCoor(location.X, location.Z);

            lander.location = new Vector3(location.X, height, location.Z);
            this.buildings.Add(lander);
            //this.RunningGameSession.simulator.AddEntity(lander);
            return lander;
        }



        public Buildings.BasicMine CreateBasicMine(Vector3 location, bool flatten)
        {
            Buildings.BasicMine mine = new Buildings.BasicMine(this.RunningGameSession);
            mine.projectionMatrix = this.projectionMatrix;
            mine.Initialize(Buildings.BasicMine.StandardScale, Buildings.BasicMine.StandardRotation);
            this.buildings.Add(mine);
            //this.RunningGameSession.simulator.AddEntity(mine);
            return mine;
        }

        public Buildings.WindTower CreateWindTower(Vector3 location, bool flatten)
        {
            Buildings.WindTower tower = new Buildings.WindTower(this.RunningGameSession);
            tower.projectionMatrix = this.projectionMatrix;
            tower.Initialize(Buildings.WindTower.StandardScale, Buildings.WindTower.StandardRotation);
            this.buildings.Add(tower);
            //this.RunningGameSession.simulator.AddEntity(tower);
            return tower;
        }

        public Buildings.Melter CreateBasicMelter(Vector3 location, bool flatten)
        {
            Buildings.Melter melter = new Buildings.Melter(this.RunningGameSession);
            melter.projectionMatrix = this.projectionMatrix;
            melter.Initialize(Buildings.Melter.StandardScale, Buildings.Melter.StandardRotation);
            this.buildings.Add(melter);
            //this.RunningGameSession.simulator.AddEntity(melter);
            return melter;
        }

        public Buildings.SolarPlant CreateBasicSolarPlant(Vector3 location, bool flatten)
        {
            Buildings.SolarPlant solar = new Buildings.SolarPlant(this.RunningGameSession);
            solar.projectionMatrix = this.projectionMatrix;
            solar.Initialize(Buildings.SolarPlant.StandardScale, Buildings.SolarPlant.StandardRotation);
            this.buildings.Add(solar);
            //this.RunningGameSession.simulator.AddEntity(solar);
            return solar;
        }


        public void RemoveBuilding(EntityBuilding building)
        {
            this.buildings.Remove(building);
        }


        private List<EntityBuilding> buildings;
        private List<Entity> Movers;

        private static EntityFactory factorySingleton;
        private Matrix projectionMatrix;
    }
}
