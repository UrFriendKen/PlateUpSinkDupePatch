using Kitchen;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenSinkDupePatch
{
    public class CleanDuplicatedItems : RestaurantSystem
    {
        EntityQuery heldByQuery;
        static List<int> uniqueHolders = new List<int>();

        protected override void Initialise()
        {
            base.Initialise();
            heldByQuery = GetEntityQuery(typeof(CItem), typeof(CHeldBy));
        }

        protected override void OnUpdate()
        {
            
            NativeArray<Entity> entities = heldByQuery.ToEntityArray(Allocator.Temp);
            uniqueHolders.Clear();
            foreach (Entity entity in entities)
            {
                if (Require(entity, out CHeldBy heldBy))
                {
                    if (heldBy.Holder == default(Entity))
                    {
                        continue;
                    }
                    if (!Require(heldBy.Holder, out CAppliance appliance) || (appliance.ID != ApplianceReferences.SinkStarting && appliance.ID != ApplianceReferences.SinkNormal))
                    {
                        continue;
                    }
                    if (!uniqueHolders.Contains(heldBy.Holder.Index))
                    {
                        uniqueHolders.Add(heldBy.Holder.Index);
                        continue;
                    }
                    if (Require(entity, out CItem item))
                    {
                        string name = "Unknown";
                        try
                        {
                            name = GDOUtils.GetExistingGDO(item.ID).name;
                        }
                        catch { }

                        Main.LogInfo($"Duplicated {name} ({item.ID}) held by same sink found!");
                        EntityManager.DestroyEntity(entity);
                        Main.LogInfo($"Destroyed duplicated item.");
                    }
                }
            }
            entities.Dispose();
        }
    }
}
