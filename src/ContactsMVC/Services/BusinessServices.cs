using ContactsMVC.CustomExceptions;
using ContactsMVC.Data;
using ContactsMVC.Models;
using ContactsMVC.ViewModels.CompanyViewModels;
using ContactsMVC.ViewModels.ContactPhoneViewModels;
using ContactsMVC.ViewModels.ContactViewModels;
using ContactsMVC.ViewModels.EventViewModels;
using ContactsMVC.ViewModels.LookupViewModels;
using ContactsMVC.ViewModels.PositionViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ContactsMVC.Enums;

namespace ContactsMVC.Services
{

    public class BusinessServices : IBusinessServices
    {

        private const string REFERENCE_CONFLICT_ERROR_MESSAGE = "conflicted with the reference constraint";
        private const string REFERENCE_CONFLICT_USER_MESSAGE = "This item cannot be deleted, because it is still being used by at least one other entity.";

        private readonly ContactsDbContext _contactsDbContext;
        private readonly IModelAdapter _modelAdapter;

        #region Constructors

        //--------------------------------------------------------------------------------------------------------------
        public BusinessServices(ContactsDbContext contactsDbContext, IModelAdapter modelAdapter)
        {
            _contactsDbContext = contactsDbContext;
            _modelAdapter = modelAdapter;
        }

        #endregion Constructors

        #region Business Methods

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<CompanyListViewModel>> GetCompanyListAsync()
        {
            IEnumerable<Company> companies = await GetCompaniesAsync();
            return _modelAdapter.ConvertCompanies(companies);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<CompanyDetailsViewModel> GetCompanyDetailsAsync(int id)
        {
            Company company = await GetCompanyAsync(id);
            return _modelAdapter.ConvertCompany(company);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<ContactListViewModel>> GetContactListAsync()
        {
            IEnumerable<Contact> contacts = await GetContactsAsync();
            return _modelAdapter.ConvertContacts(contacts);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<ContactListViewModel>> GetContactListForCompanyAsync(int companyID)
        {
            IEnumerable<Contact> contacts = await GetContactsAsync();
            IEnumerable<Contact> contactsForCompany = null;
            if (contacts != null)
            {
                contactsForCompany = contacts.Where(ct => ct.CompanyID == companyID).ToList();
            }
            return _modelAdapter.ConvertContacts(contactsForCompany);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<ContactDetailsViewModel> GetContactDetailsAsync(int id)
        {
            Contact contact = await GetContactAsync(id);
            return _modelAdapter.ConvertContact(contact);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<ContactPhoneViewModel> GetContactPhoneDetailsAsync(int id)
        {
            ContactPhone contactPhone = await GetContactPhoneAsync(id);
            return _modelAdapter.ConvertContactPhone(contactPhone);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<EventDetailsViewModel> GetEventDetailsAsync(int id)
        {
            Event ev = await GetEventAsync(id);
            return _modelAdapter.ConvertEvent(ev);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<LookupItemViewModel> GetLookupItemAsync(LookupType lookupType, int id)
        {

            LookupItem lookupItem = null;

            switch (lookupType)
            {
                case LookupType.ContactType:
                    lookupItem = await _contactsDbContext.ContactTypes.AsNoTracking().SingleOrDefaultAsync(item => item.ID == id);
                    break;
                case LookupType.EventType:
                    lookupItem = await _contactsDbContext.EventTypes.AsNoTracking().SingleOrDefaultAsync(item => item.ID == id);
                    break;
                case LookupType.PhoneType:
                    lookupItem = await _contactsDbContext.ContactPhoneTypes.AsNoTracking().SingleOrDefaultAsync(item => item.ID == id);
                    break;
                default:
                    break;
            }

            return _modelAdapter.ConvertLookupItem(lookupItem);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<LookupItemViewModel>> GetLookupListAsync(LookupType lookupType)
        {

            IEnumerable<LookupItem> lookupItems = new List<LookupItem>();

            switch (lookupType)
            {
                case LookupType.ContactType:
                    lookupItems = await GetContactTypesAsync();
                    break;
                case LookupType.EventType:
                    lookupItems = await GetEventTypesAsync();
                    break;
                case LookupType.PhoneType:
                    lookupItems = await GetContactPhoneTypesAsync();
                    break;
                default:
                    break;
            }

            return _modelAdapter.ConvertLookupList(lookupItems);

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<PositionListViewModel>> GetPositionListAsync()
        {
            IEnumerable<Position> positions = await GetPositionsAsync();
            return _modelAdapter.ConvertPositions(positions);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<PositionDetailsViewModel> GetPositionDetailsAsync(int id)
        {
            Position position = await GetPositionAsync(id);
            return _modelAdapter.ConvertPosition(position);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<StateListViewModel>> GetStateListAsync()
        {
            IEnumerable<State> states = await GetStatesAsync();
            return _modelAdapter.ConvertStates(states);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreateCompanyAsync(CompanyDetailsViewModel companyDetailsViewModel)
        {

            Company newCompany = _modelAdapter.ConvertCompany(companyDetailsViewModel);

            Company existingCompanyWithSameName = await _contactsDbContext.Companies
                                                                          .AsNoTracking()
                                                                          .Where(c => (c.Name ?? "").ToUpper() == (newCompany.Name ?? "").ToUpper())
                                                                          .FirstOrDefaultAsync();

            if (existingCompanyWithSameName != null)
            {
                throw new BusinessException("Could not create new company because the specified company name is already in use.");
            }

            _contactsDbContext.Add(newCompany);
            await _contactsDbContext.SaveChangesAsync();

            return newCompany.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreateContactAsync(ContactDetailsViewModel contactDetailsViewModel)
        {

            Contact newContact = _modelAdapter.ConvertContact(contactDetailsViewModel);

            _contactsDbContext.Add(newContact);
            await _contactsDbContext.SaveChangesAsync();

            return newContact.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreateContactPhoneAsync(ContactPhoneViewModel contactPhoneViewModel)
        {

            ContactPhone newContactPhone = _modelAdapter.ConvertContactPhone(contactPhoneViewModel);

            // Check whether this contact already has a phone number marked as primary; if not, make this one primary
            ContactPhone existingPrimary = await _contactsDbContext.ContactPhones
                                                                   .AsNoTracking()
                                                                   .FirstOrDefaultAsync(cp => cp.ContactID == contactPhoneViewModel.ContactID && cp.IsPrimaryPhone == true);
            if (existingPrimary == null)
            {
                newContactPhone.IsPrimaryPhone = true;
            }

            _contactsDbContext.Add(newContactPhone);
            await _contactsDbContext.SaveChangesAsync();

            return newContactPhone.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreateEventAsync(EventDetailsViewModel eventDetailsViewModel)
        {

            Event newEvent = _modelAdapter.ConvertEvent(eventDetailsViewModel);
            _contactsDbContext.Add(newEvent);
            await _contactsDbContext.SaveChangesAsync();
            return newEvent.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreateLookupItemAsync(LookupType lookupType, LookupItemViewModel lookupItemViewModel)
        {

            LookupItem newLookupItem = _modelAdapter.ConvertLookupItem(lookupItemViewModel, lookupType);
            _contactsDbContext.Add(newLookupItem);
            await _contactsDbContext.SaveChangesAsync();
            return newLookupItem.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<int> CreatePositionAsync(PositionDetailsViewModel positionDetailsViewModel)
        {

            Position newPosition = _modelAdapter.ConvertPosition(positionDetailsViewModel);
            _contactsDbContext.Add(newPosition);
            await _contactsDbContext.SaveChangesAsync();
            return newPosition.ID;

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteCompanyAsync(int id)
        {
            Company companyToDelete = _modelAdapter.GetCompany(id);
            await DeleteEntityAsync(companyToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteContactAsync(int id)
        {
            Contact contactToDelete = _modelAdapter.GetContact(id);
            await DeleteEntityAsync(contactToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteContactPhoneAsync(int id)
        {
            ContactPhone contactPhoneToDelete = _modelAdapter.GetContactPhone(id);
            await DeleteEntityAsync(contactPhoneToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteEventAsync(int id)
        {
            Event eventToDelete = _modelAdapter.GetEvent(id);
            await DeleteEntityAsync(eventToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteLookupItemAsync(LookupType lookupType, int id)
        {
            LookupItem lookupItemToDelete = _modelAdapter.GetLookupItem(id, lookupType);
            await DeleteEntityAsync(lookupItemToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeletePositionAsync(int id)
        {
            Position positionToDelete = _modelAdapter.GetPosition(id);
            await DeleteEntityAsync(positionToDelete);
        }

        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Set the specified contact phone to be the primary for its contact.
        /// Any others will be set as non-primary.
        /// </summary>
        public async Task SetContactPhoneAsPrimaryAsync(int contactPhoneID)
        {

            int contactID = await _contactsDbContext.ContactPhones
                                                    .AsNoTracking()
                                                    .Where(cp => cp.ID == contactPhoneID)
                                                    .Select(cp => cp.ContactID)
                                                    .SingleOrDefaultAsync();

            Contact contact = await _contactsDbContext.Contacts
                                                      .Include(ct => ct.ContactPhones)
                                                      .SingleOrDefaultAsync(ct => ct.ID == contactID);

            if (contact == null)
            {
                return;
            }

            foreach (ContactPhone contactPhone in contact.ContactPhones)
            {
                contactPhone.IsPrimaryPhone = (contactPhone.ID == contactPhoneID);
                _contactsDbContext.Entry(contactPhone).State = EntityState.Modified;
            }

            _contactsDbContext.SaveChanges();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateCompanyAsync(CompanyDetailsViewModel companyDetailsViewModel)
        {

            Company companyToUpdate = _modelAdapter.ConvertCompany(companyDetailsViewModel);

            Company existingCompanyWithSameName = await _contactsDbContext.Companies
                                                                          .AsNoTracking()
                                                                          .Where(c => (c.Name ?? "").ToUpper() == (companyToUpdate.Name ?? "").ToUpper() && c.ID != companyToUpdate.ID)
                                                                          .FirstOrDefaultAsync();

            if (existingCompanyWithSameName != null)
            {
                throw new BusinessException("Could not update because the specified company name is already in use.");
            }

            _contactsDbContext.Update(companyToUpdate);
            await _contactsDbContext.SaveChangesAsync();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateContactAsync(ContactDetailsViewModel contactDetailsViewModel)
        {

            Contact contactToUpdate = _modelAdapter.ConvertContact(contactDetailsViewModel);
            _contactsDbContext.Update(contactToUpdate);
            await _contactsDbContext.SaveChangesAsync();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateContactPhoneAsync(ContactPhoneViewModel contactPhoneViewModel)
        {

            ContactPhone contactPhoneToUpdate = _modelAdapter.ConvertContactPhone(contactPhoneViewModel);

            // If the contact has no existing phones, mark the new one as primary
            ContactPhone existingContactPhone = _contactsDbContext.ContactPhones
                                                                  .AsNoTracking()
                                                                  .SingleOrDefault(cp => cp.ID == contactPhoneToUpdate.ID);
            if (existingContactPhone != null)
            {
                contactPhoneToUpdate.IsPrimaryPhone = existingContactPhone.IsPrimaryPhone;
            }

            _contactsDbContext.Update(contactPhoneToUpdate);
            await _contactsDbContext.SaveChangesAsync();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateEventAsync(EventDetailsViewModel eventDetailsViewModel)
        {

            Event eventToUpdate = _modelAdapter.ConvertEvent(eventDetailsViewModel);
            _contactsDbContext.Update(eventToUpdate);
            await _contactsDbContext.SaveChangesAsync();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateLookupItemAsync(LookupType lookupType, LookupItemViewModel lookupItemViewModel)
        {

            LookupItem lookupItemToUpdate = _modelAdapter.ConvertLookupItem(lookupItemViewModel, lookupType);
            _contactsDbContext.Update(lookupItemToUpdate);
            await _contactsDbContext.SaveChangesAsync();

        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdatePositionAsync(PositionDetailsViewModel positionDetailsViewModel)
        {

            Position newPosition = _modelAdapter.ConvertPosition(positionDetailsViewModel);

            _contactsDbContext.Update(newPosition);

            List<int> existingContactIDs = await _contactsDbContext.PositionContacts
                                                                  .AsNoTracking()
                                                                  .Where(pc => pc.PositionID == positionDetailsViewModel.ID)
                                                                  .Select(pc => pc.ContactID)
                                                                  .ToListAsync();

            List<int> validContactsForCompany = await _contactsDbContext.Contacts
                                                                        .AsNoTracking()
                                                                        .Where(ct => ct.CompanyID == positionDetailsViewModel.CompanyID)
                                                                        .Select(ct => ct.ID)
                                                                        .ToListAsync();

            // Add any new PositionContacts not already in the database
            foreach (PositionContact pc in newPosition.PositionContacts)
            {
                if (!validContactsForCompany.Contains(pc.ContactID))
                {
                    continue;       // Don't add any IDs that belong to Contacts at a different company
                }
                if (!existingContactIDs.Contains(pc.ContactID))
                {
                    _contactsDbContext.Entry(pc).State = EntityState.Added;
                }
            }

            // Remove any existing PositionContacts from the database as necessary
            foreach (int contactID in existingContactIDs)
            {
                PositionContact pc = newPosition.PositionContacts.FirstOrDefault(o => o.ContactID == contactID);
                if (pc == null)
                {
                    // This PositionContact is in the database, but not in the new collection, so mark it for deletion
                    PositionContact pcToRemove = new PositionContact() { PositionID = newPosition.ID, ContactID = contactID };
                    newPosition.PositionContacts.Add(pcToRemove);
                    _contactsDbContext.Remove(pcToRemove);
                }
                else
                {
                    // This PositionContact is in the database and in the new collection.
                    // Mark it for deletion if it is for a contact at another company
                    if (!validContactsForCompany.Contains(contactID)) {
                        _contactsDbContext.Remove(pc);
                    }
                }
            }
            
            await _contactsDbContext.SaveChangesAsync();

        }

        #endregion Business Methods

        #region Repository Methods

        //--------------------------------------------------------------------------------------------------------------
        private async Task DeleteEntityAsync(object entityToDelete)
        {

            try
            {
                _contactsDbContext.Remove(entityToDelete);
                await _contactsDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException duex)
            {
                if (duex.InnerException != null && duex.InnerException.GetType() == typeof(System.Data.SqlClient.SqlException))
                {
                    if ((duex.InnerException.Message ?? string.Empty).ToLower().Contains(REFERENCE_CONFLICT_ERROR_MESSAGE))
                    {
                        // In this scenario, throw a BusinessException so our specific user-friendly error message
                        // will get displayed by the UI
                        throw new BusinessException(REFERENCE_CONFLICT_USER_MESSAGE);
                    }
                }
                else
                {
                    throw duex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<Company>> GetCompaniesAsync()
        {
            return await _contactsDbContext.Companies
                                           .AsNoTracking()
                                           .OrderBy(c => c.Name)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<Company> GetCompanyAsync(int id)
        {
            return await _contactsDbContext.Companies
                                           .AsNoTracking()
                                           .Include(c => c.Positions)
                                               .ThenInclude(p => p.Events)
                                                   .ThenInclude(ev => ev.EventType)
                                           .Include(c => c.Contacts)
                                               .ThenInclude(ct => ct.ContactType)
                                           .Include(c => c.Contacts)
                                               .ThenInclude(ct => ct.ContactPhones)
                                                   .ThenInclude(cp => cp.ContactPhoneType)
                                           .SingleOrDefaultAsync(c => c.ID == id);
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<Contact>> GetContactsAsync()
        {
            return await _contactsDbContext.Contacts
                                           .AsNoTracking()
                                           .Include(ct => ct.Company)
                                           .Include(ct => ct.ContactType)
                                           .Include(ct => ct.ContactPhones)
                                               .ThenInclude(cp => cp.ContactPhoneType)
                                           .OrderBy(ct => ct.Company.Name)
                                           .ThenBy(ct => ct.LastName)
                                           .ThenBy(ct => ct.FirstName)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<Contact> GetContactAsync(int id)
        {
            return await _contactsDbContext.Contacts
                                           .AsNoTracking()
                                           .Include(ct => ct.Company)
                                           .Include(ct => ct.ContactType)
                                           .Include(ct => ct.ContactPhones)
                                               .ThenInclude(cp => cp.ContactPhoneType)
                                           .OrderBy(ct => ct.Company.Name)
                                           .ThenBy(ct => ct.LastName)
                                           .ThenBy(ct => ct.FirstName)
                                           .SingleOrDefaultAsync(ct => ct.ID == id);
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<ContactType>> GetContactTypesAsync()
        {
            return await _contactsDbContext.ContactTypes
                                           .AsNoTracking()
                                           .OrderBy(et => et.Sequence)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<ContactPhoneType>> GetContactPhoneTypesAsync()
        {
            return await _contactsDbContext.ContactPhoneTypes
                                           .AsNoTracking()
                                           .OrderBy(et => et.Sequence)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<ContactPhone> GetContactPhoneAsync(int id)
        {
            return await _contactsDbContext.ContactPhones
                                           .AsNoTracking()
                                           .Include(cp => cp.ContactPhoneType)
                                           .Include(cp => cp.Contact)
                                           .SingleOrDefaultAsync(cp => cp.ID == id);
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<Event> GetEventAsync(int id)
        {
            return await _contactsDbContext.Events
                                           .AsNoTracking()
                                           .Include(e => e.EventType)
                                           .Include(e => e.Position)
                                               .ThenInclude(p => p.Company)
                                           .SingleOrDefaultAsync(e => e.ID == id);
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<EventType>> GetEventTypesAsync()
        {
            return await _contactsDbContext.EventTypes
                                           .AsNoTracking()
                                           .OrderBy(et => et.Sequence)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<Position>> GetPositionsAsync()
        {
            return await _contactsDbContext.Positions
                                           .AsNoTracking()
                                           .Include(p => p.Company)
                                           .Include(p => p.Events)
                                               .ThenInclude(ev => ev.EventType)
                                           .OrderBy(p => p.Company.Name)
                                           .ThenBy(p => p.Title)
                                           .ToListAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<Position> GetPositionAsync(int id)
        {
            return await _contactsDbContext.Positions
                                           .AsNoTracking()
                                           .Include(p => p.Events)
                                               .ThenInclude(e => e.EventType)
                                           .Include(p => p.Company)
                                           .Include(p => p.PositionContacts)
                                               .ThenInclude(pc => pc.Contact)
                                                   .ThenInclude(ct => ct.Company)
                                           .Include(p => p.PositionContacts)
                                               .ThenInclude(pc => pc.Contact)
                                                   .ThenInclude(ct => ct.ContactType)
                                           .Include(p => p.PositionContacts)
                                               .ThenInclude(pc => pc.Contact)
                                                   .ThenInclude(ct => ct.ContactPhones)
                                                       .ThenInclude(cp => cp.ContactPhoneType)
                                           .SingleOrDefaultAsync(p => p.ID == id);
        }

        //--------------------------------------------------------------------------------------------------------------
        private async Task<List<State>> GetStatesAsync()
        {
            return await _contactsDbContext.States
                                           .AsNoTracking()
                                           .OrderBy(s => s.Abbreviation)
                                           .ToListAsync();
        }

        #endregion Repository Methods

    }

}
