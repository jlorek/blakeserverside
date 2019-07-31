using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bogus;

namespace BlakeServerSide.Data
{
    public class IdentityService
    {
        public string Name { get; }

        public IdentityService() {
            var faker = new Faker();
            Name = faker.Name.FirstName();
        }
    }
}
