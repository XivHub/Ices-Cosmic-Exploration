using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.GameHelpers;
using System.Collections.Generic;

namespace ICE.Utilities.GatheringHelper.RouteLoader;

/// <summary>
/// Shared helpers for deduplicating and building gathering routes from live scene data.
/// </summary>
public static class RouteSynthesizer
{
    /// <summary>
    /// Adds <paramref name="candidate"/> to <paramref name="nodes"/> if no matching entry exists.
    /// When <paramref name="byPosition"/> is true, matches by rounded XZ within 1.0f;
    /// otherwise matches by NodeId. Returns false (no-op) if a duplicate is found.
    /// </summary>
    public static bool MergeNode(List<NodeInfo> nodes, NodeInfo candidate, bool byPosition)
    {
        foreach (var existing in nodes)
        {
            if (!byPosition)
            {
                if (existing.NodeId == candidate.NodeId)
                    return false;
            }
            else
            {
                float dx = MathF.Abs(MathF.Round(existing.Position.X) - MathF.Round(candidate.Position.X));
                float dz = MathF.Abs(MathF.Round(existing.Position.Z) - MathF.Round(candidate.Position.Z));
                if (dx <= 1.0f && dz <= 1.0f)
                    return false;
            }
        }

        nodes.Add(candidate);
        return true;
    }

    /// <summary>
    /// Scans <see cref="Svc.Objects"/> for targetable <see cref="ObjectKind.GatheringPoint"/>
    /// objects within <paramref name="radius"/> of the player and merges each into
    /// <paramref name="nodes"/> via <see cref="MergeNode"/>. Returns the count of newly added nodes.
    /// </summary>
    public static int CaptureNearbyTargetable(List<NodeInfo> nodes, float radius)
    {
        int added = 0;

        foreach (var obj in Svc.Objects)
        {
            if (obj.ObjectKind != ObjectKind.GatheringPoint)
                continue;
            if (!obj.IsTargetable)
                continue;
            if (Vector3.Distance(obj.Position, Player.Position) > radius)
                continue;

            var nearestLand = P.Navmesh.Installed && P.Navmesh.IsReady()
                ? P.Navmesh.NearestPointReachable(obj.Position, 3f, 5f)
                : null;

            var candidate = new NodeInfo
            {
                NodeId      = obj.BaseId,
                Position    = obj.Position,
                LandZone    = nearestLand ?? Player.Position,
                RadiusStart = 0f,
                RadiusEnd   = 359f,
                MinDistance = 1f,
                MaxDistance = 3f,
                FanHeight   = 0f,
            };

            if (MergeNode(nodes, candidate, C.LearnByPosition))
                added++;
        }

        return added;
    }

    /// <summary>
    /// Reorders <paramref name="nodes"/> in place using a greedy nearest-neighbour heuristic
    /// starting from <paramref name="start"/>. Ordering is by LandZone distance.
    /// </summary>
    public static void GreedyNearestNeighborOrder(List<NodeInfo> nodes, Vector3 start)
    {
        if (nodes.Count <= 1)
            return;

        var ordered = new List<NodeInfo>(nodes.Count);
        var remaining = new List<NodeInfo>(nodes);
        var current = start;

        while (remaining.Count > 0)
        {
            int bestIdx = 0;
            float bestDist = Vector3.Distance(current, remaining[0].LandZone);

            for (int i = 1; i < remaining.Count; i++)
            {
                float d = Vector3.Distance(current, remaining[i].LandZone);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }

            ordered.Add(remaining[bestIdx]);
            current = remaining[bestIdx].LandZone;
            remaining.RemoveAt(bestIdx);
        }

        nodes.Clear();
        nodes.AddRange(ordered);
    }

    /// <summary>
    /// Returns all (routeId, nodeId) pairs from <see cref="GatheringRouteLoader.LoadedRoutes"/>
    /// where a node whose <c>NodeId</c> is in <paramref name="nodeIds"/> appears in a route
    /// other than <paramref name="thisRouteId"/>.
    /// </summary>
    public static List<(uint routeId, uint nodeId)> FindNodeIdConflicts(uint thisRouteId, IEnumerable<uint> nodeIds)
    {
        var conflicts = new List<(uint routeId, uint nodeId)>();
        var nodeIdSet = new HashSet<uint>(nodeIds);

        foreach (var (routeId, route) in GatheringRouteLoader.LoadedRoutes)
        {
            if (routeId == thisRouteId)
                continue;
            if (route.Nodes == null)
                continue;

            foreach (var node in route.Nodes)
            {
                if (nodeIdSet.Contains(node.NodeId))
                    conflicts.Add((routeId, node.NodeId));
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Scans <see cref="Svc.Objects"/> for targetable gathering points within <paramref name="radius"/>
    /// of the player, merges each into the route identified by (<paramref name="routeId"/>,
    /// <paramref name="territoryId"/>, <paramref name="jobId"/>), and persists when the route
    /// reaches 3 or more nodes.
    /// Returns the number of nodes newly added this call.
    /// </summary>
    public static int SynthesizeRoute(uint routeId, uint territoryId, uint jobId, float radius)
    {
        var route = GatheringRouteLoader.GetOrCreateRoute(routeId, territoryId, jobId);

        var nodes = route.Nodes == null ? new List<NodeInfo>() : new List<NodeInfo>(route.Nodes);

        int added = CaptureNearbyTargetable(nodes, radius);

        route.Nodes = nodes;

        if (route.Nodes.Count >= 3)
            GatheringRouteLoader.SaveRoute(route);

        return added;
    }
}
