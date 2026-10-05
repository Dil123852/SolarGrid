/*
 * File: MongoMappings.cs
 * Purpose: BSON class maps for the domain entities, so the Domain project needs no MongoDB
 *          attributes. NIC is a string _id; other ids are ObjectIds exposed as strings;
 *          enums are stored as strings ("Pending", "Backoffice") for readable documents.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence
{
    public static class MongoMappings
    {
        private static readonly object Gate = new();
        private static bool _registered;

        // Registers BSON class maps and conventions once per process (thread-safe).
        public static void Register()
        {
            lock (Gate)
            {
                if (_registered) return;

                ConventionRegistry.Register("SolarGridConventions", new ConventionPack
                {
                    new EnumRepresentationConvention(BsonType.String),
                    new IgnoreExtraElementsConvention(true)
                }, _ => true);

                BsonClassMap.RegisterClassMap<Prosumer>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(p => p.NIC).SetSerializer(new StringSerializer(BsonType.String));
                });

                BsonClassMap.RegisterClassMap<User>(MapObjectId<User>(u => u.Id));
                BsonClassMap.RegisterClassMap<MicrogridNode>(MapObjectId<MicrogridNode>(n => n.Id));
                BsonClassMap.RegisterClassMap<EnergyReservation>(MapObjectId<EnergyReservation>(r => r.Id));

                _registered = true;
            }
        }

        // AutoMap plus a database-generated ObjectId surfaced as a string property.
        private static Action<BsonClassMap<T>> MapObjectId<T>(System.Linq.Expressions.Expression<Func<T, string>> id) =>
            map =>
            {
                map.AutoMap();
                map.MapIdMember(id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId));
            };
    }
}
