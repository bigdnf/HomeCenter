﻿using HomeCenter.Services.MotionService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HomeCenter.Extensions;
using System.Collections.ObjectModel;

namespace HomeCenter.Services.MotionService
{
    internal class RoomDictionary
    {
        private readonly IReadOnlyDictionary<string, Room> _rooms;
        private readonly IReadOnlyDictionary<string, ReadOnlyDictionary<string, Room>> _neighbors;

        private readonly MotionConfiguration _motionConfiguration;

        public Room this[string uid] => _rooms[uid];

        private Room this[MotionWindow window] => _rooms[window.Start.Uid];

        private Room this[MotionVector vector] => _rooms[vector.EndPoint];

        public RoomDictionary(IEnumerable<Room> rooms, IReadOnlyDictionary<string, ReadOnlyDictionary<string, Room>> neighbors, MotionConfiguration motionConfiguration)
        {
            _rooms = rooms.ToDictionary(k => k.Uid, v => v).AsReadOnly();
            _motionConfiguration = motionConfiguration;
            _neighbors = neighbors;
        }

        public int NumberOfPersons() => _rooms.Sum(md => md.Value.NumberOfPersons);

        /// <summary>
        /// Check if two point in time can physically be a proper vector
        /// </summary>
        public bool IsProperVector(MotionPoint start, MotionPoint potencialEnd)
        {
            return AreNeighbors(start, potencialEnd) && potencialEnd.IsMovePhisicallyPosible(start, _motionConfiguration.MotionMinDiff);
        }

        public async Task HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0) return;

            var targetRoom = this[motionVectors[0]];
            await targetRoom.HandleVectors(motionVectors);
        }

        public Task MarkMotion(MotionWindow point)
        {
            return this[point].MarkMotion(point.Start.TimeStamp);
        }

        /// <summary>
        /// Evaluates each room state
        /// </summary>
        public async Task CheckRooms(DateTimeOffset motionTime)
        {
            await _rooms.Values.Select(r => r.EvaluateConfusions(motionTime)).WhenAll();

            await _rooms.Values.Select(r => r.PeriodicUpdate(motionTime)).WhenAll();
        }

        /// <summary>
        /// Check if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _neighbors[p1.Uid].ContainsKey(p2.Uid);


        /// <summary>
        /// Checks if there was any move in current room and all neighbors excluding <paramref name="roomToExclude"/> after <paramref name="referenceTime"/>
        /// </summary>
        public bool MoveInNeighborhood(string roomid, string roomToExclude, DateTimeOffset referenceTime)
        {
            return _neighbors[roomid].Values
                                     .Where(r => r.Uid != roomToExclude)
                                     .Any(n => n.RoomStatistic.LastMotion.Time > referenceTime) || _rooms[roomid].RoomStatistic.LastMotion.Time > referenceTime;
        }
    }
}