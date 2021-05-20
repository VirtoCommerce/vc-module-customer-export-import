using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CustomerExportPagedDataSourceTests
    {
        [Fact]
        public async Task GetTotalCount_Calculate_AndReturnTotalCount()
        {
            // Arrange
            var contacts = new Member[] { new Contact(), new Contact() };
            var customerExportPagedDataSourceFactory = GetCustomerExportPagedDataSourceFactory(contacts, Array.Empty<Member>());
            var customerExportPagedDataSource = customerExportPagedDataSourceFactory.Create(10, new ExportDataRequest());

            // Act
            var totalCount = await customerExportPagedDataSource.GetTotalCountAsync();

            // Assert
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task FetchAsync_LoadContactParentOrganization_WillUseExisting()
        {
            // Arrange
            var contact = new Contact { Id = "Contact1", Organizations = new[] { "Organization1" } };
            var organization = new Organization { Id = "Organization1", Name = "Organization 1 Name" };
            var members = new Member[] { contact, organization };
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource(members, pageSize: 10);

            // Act
            await customerExportPagedDataSource.FetchAsync();
            var exportedContact = (ExportableContact)customerExportPagedDataSource.Items.First();

            // Assert
            Assert.Equal(organization.Id, exportedContact.OrganizationId);
            Assert.Equal(organization.Name, exportedContact.OrganizationName);
        }

        [Fact]
        public async Task FetchAsync_MultipleTimes_WillUpdateCurrentPageNumber()
        {
            // Arrange
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource();

            // Act
            await customerExportPagedDataSource.FetchAsync();
            await customerExportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal(2, customerExportPagedDataSource.CurrentPageNumber);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_ReturnsOnlyRequestedNumberOfItems()
        {
            // Arrange
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource();

            // Act
            await customerExportPagedDataSource.FetchAsync();

            // Assert
            Assert.Single(customerExportPagedDataSource.Items);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfTheSource_WillReturnTrue()
        {
            // Arrange
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource();

            // Act
            var result = await customerExportPagedDataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfTheSource_WillReturnFalse()
        {
            // Arrange
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource();

            // Act
            await customerExportPagedDataSource.FetchAsync();
            await customerExportPagedDataSource.FetchAsync();
            var result = await customerExportPagedDataSource.FetchAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfTheSource_WillFetchNoItems()
        {
            // Arrange
            var customerExportPagedDataSource = GetCustomerExportPagedDataSource();

            // Act
            await customerExportPagedDataSource.FetchAsync();
            await customerExportPagedDataSource.FetchAsync();
            await customerExportPagedDataSource.FetchAsync();

            // Assert
            Assert.Empty(customerExportPagedDataSource.Items);
        }

        private static IMemberService GetMemberService(Member[] members)
        {
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(service => service.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(() => Task.FromResult(members));
            return memberServiceMock.Object;
        }

        private static IMemberSearchService GetMemberSearchService(Member[] members)
        {
            var memberSearchServiceMock = new Mock<IMemberSearchService>();
            memberSearchServiceMock.Setup(service => service.SearchMembersAsync(It.IsAny<MembersSearchCriteria>()))
                .Returns<MembersSearchCriteria>(memberSearchCriteria =>
                    Task.FromResult(new MemberSearchResult { Results = members.Skip(memberSearchCriteria.Skip).Take(memberSearchCriteria.Take).ToArray(), TotalCount = members.Length }));
            return memberSearchServiceMock.Object;
        }

        private static IStoreService GetStoreService()
        {
            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(service => service.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new[]
                {
                    new Store { Id = "Store", Name = "Store" }
                }));
            return storeServiceMock.Object;
        }

        private static ICustomerExportPagedDataSourceFactory GetCustomerExportPagedDataSourceFactory(Member[] members, Member[] additionalOrganizations)
        {
            var memberService = GetMemberService(additionalOrganizations);
            var membersSearchService = GetMemberSearchService(members);
            return new CustomerExportPagedDataSourceFactory(memberService, membersSearchService, GetStoreService());
        }

        private static ICustomerExportPagedDataSource GetCustomerExportPagedDataSource(Member[] members = null, Member[] additionalOrganizations = null, int pageSize = 1)
        {
            members ??= new Member[] { new Contact { Id = "Contact1" }, new Organization { Id = "Organization1" } };
            additionalOrganizations ??= Array.Empty<Member>();
            
            var customerExportPagedDataSourceFactory = GetCustomerExportPagedDataSourceFactory(members, additionalOrganizations);
            var customerExportPagedDataSource = customerExportPagedDataSourceFactory.Create(pageSize, new ExportDataRequest());
            return customerExportPagedDataSource;
        }
    }
}
