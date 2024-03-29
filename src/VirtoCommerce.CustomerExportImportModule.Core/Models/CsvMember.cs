using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public abstract class CsvMember : IExportable, IHasOuterId, IHasDynamicProperties
    {
        public abstract string Id { get; set; }

        public abstract string OuterId { get; set; }

        [Optional]
        [Name("Phones")]
        public virtual string Phones { get; set; }

        [Optional]
        [Name("Emails")]
        public virtual string Emails { get; set; }

        [Optional]
        [Name("Address Type")]
        public virtual string AddressType { get; set; }

        [Optional]
        [Name("Address First Name")]
        public virtual string AddressFirstName { get; set; }

        [Optional]
        [Name("Address Last Name")]
        public virtual string AddressLastName { get; set; }

        [Optional]
        [Name("Address Country")]
        public virtual string AddressCountry { get; set; }

        [Optional]
        [Name("Address Country Code")]
        public virtual string AddressCountryCode { get; set; }

        [Optional]
        [Name("Address Region")]
        public virtual string AddressRegion { get; set; }

        [Optional]
        [Name("Address Region Code")]
        public virtual string AddressRegionCode { get; set; }

        [Optional]
        [Name("Address City")]
        public virtual string AddressCity { get; set; }

        [Optional]
        [Name("Address Line1")]
        public virtual string AddressLine1 { get; set; }

        [Optional]
        [Name("Address Line2")]
        public virtual string AddressLine2 { get; set; }

        [Optional]
        [Name("Address Zip Code")]
        public virtual string AddressZipCode { get; set; }

        [Optional]
        [Name("Address Email")]
        public virtual string AddressEmail { get; set; }

        [Optional]
        [Name("Address Phone")]
        public virtual string AddressPhone { get; set; }

        [Optional]
        [Name("Additional Line")]
        [BooleanTrueValues("True", "Yes")]
        [BooleanFalseValues("False", "No")]
        public virtual bool? AdditionalLine { get; set; }

        [Ignore, JsonIgnore]
        public string ObjectType { get; set; }

        [Ignore, JsonIgnore]
        public bool AddressIsEmpty =>
            string.IsNullOrEmpty(AddressType) && string.IsNullOrEmpty(AddressFirstName) && string.IsNullOrEmpty(AddressLastName) &&
            string.IsNullOrEmpty(AddressCountry) && string.IsNullOrEmpty(AddressCountryCode) &&
            string.IsNullOrEmpty(AddressRegion) && string.IsNullOrEmpty(AddressRegionCode) &&
            string.IsNullOrEmpty(AddressCity) && string.IsNullOrEmpty(AddressLine1) && string.IsNullOrEmpty(AddressLine2) &&
            string.IsNullOrEmpty(AddressZipCode) && string.IsNullOrEmpty(AddressEmail) && string.IsNullOrEmpty(AddressPhone);

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }

        public bool IdsEquals(Member member) =>
            (!string.IsNullOrEmpty(Id) && Id.EqualsInvariant(member.Id))
            || (!string.IsNullOrEmpty(OuterId) && OuterId.EqualsInvariant(member.OuterId));

        public static bool IdsEquals(string id, string outerId, Member member) =>
            (!string.IsNullOrEmpty(id) && id.EqualsInvariant(member.Id))
            || (!string.IsNullOrEmpty(outerId) && outerId.EqualsInvariant(member.OuterId));


        protected void PatchDynamicProperties(Member target)
        {
            target.DynamicProperties ??= new List<DynamicObjectProperty>();

            if (DynamicProperties?.Count > 0)
            {
                foreach (var property in DynamicProperties)
                {
                    var targetProperty = target.DynamicProperties.FirstOrDefault(x => x.Name == property.Name);
                    if (targetProperty == null)
                    {
                        target.DynamicProperties.Add(property);
                    }
                    else
                    {
                        targetProperty.Values = property.Values;
                    }
                }
            }
        }

        private static readonly IEqualityComparer<Address> _addressEqualityComparer = AnonymousComparer.Create(
            (Address x) => $"{x.AddressType:F}:{x.FirstName}:{x.LastName}:{x.CountryCode.IfNullOrEmpty(x.CountryName)}:{x.RegionId.IfNullOrEmpty(x.RegionName)}:{x.City}:{x.Line1}:{x.Line2}:{x.PostalCode}:{x.Email}:{x.Phone}", StringComparer.OrdinalIgnoreCase);

        protected void PatchAddresses(Member target)
        {
            target.Addresses ??= new List<Address>();
            if (AddressIsEmpty)
            {
                return;
            }

            var address = AbstractTypeFactory<Address>.TryCreateInstance();
            address.AddressType = EnumUtility.SafeParseFlags(AddressType, CoreModule.Core.Common.AddressType.BillingAndShipping);
            address.FirstName = AddressFirstName;
            address.LastName = AddressLastName;
            address.CountryName = AddressCountry;
            address.CountryCode = AddressCountryCode;
            address.RegionName = AddressRegion;
            address.RegionId = AddressRegionCode?.EmptyToNull();
            address.City = AddressCity;
            address.Line1 = AddressLine1;
            address.Line2 = AddressLine2;
            address.PostalCode = AddressZipCode;
            address.Email = AddressEmail;
            address.Phone = AddressPhone;

            if (address.AddressType != CoreModule.Core.Common.AddressType.BillingAndShipping && !target.Addresses.Any(x => x.AddressType == address.AddressType && x.IsDefault))
            {
                address.IsDefault = true;
            }

            var existedAddress = target.Addresses.FirstOrDefault(x => _addressEqualityComparer.Equals(x, address));
            if (existedAddress == null)
            {
                target.Addresses.Add(address);
            }
            else
            {
                UpdateExistedAddress(address, existedAddress);
            }
        }

        private static void UpdateExistedAddress(Address address, Address existedAddress)
        {
            if (address.IsDefault)
            {
                existedAddress.IsDefault = true;
            }

            //Due to realization of _addressEqualityComparer need to check CountryCode and RegionId
            if (!address.CountryCode.IsNullOrEmpty() && existedAddress.CountryCode.IsNullOrEmpty())
            {
                existedAddress.CountryCode = address.CountryCode;
            }
            if (!address.RegionId.IsNullOrEmpty() && existedAddress.RegionId.IsNullOrEmpty())
            {
                existedAddress.RegionId = address.RegionId;
            }
        }
    }
}
