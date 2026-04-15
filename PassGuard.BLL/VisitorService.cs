using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class VisitorService
    {
        private readonly VisitorRepository _repo;

        public VisitorService(VisitorRepository repo)
        {
            _repo = repo;
        }

        public Visitor? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public Visitor? GetByFullNameAndPhone(string fullName, string phone)
        {
            return _repo.GetByFullNameAndPhone(fullName, phone);
        }

        public Visitor? GetByIdForCreator(int id, string createdByUserId)
        {
            return _repo.GetByIdForCreator(id, createdByUserId);
        }

        public List<Visitor> GetAll()
        {
            return _repo.GetAll();
        }

        public List<Visitor> GetByCreatedByUserId(string createdByUserId)
        {
            return _repo.GetByCreatedByUserId(createdByUserId);
        }

        public void Add(Visitor visitor)
        {
            _repo.Add(visitor);
        }

        public void Update(Visitor visitor)
        {
            _repo.Update(visitor);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public bool ExistsByFullNameAndPhone(string fullName, string phone, int? ignoreVisitorId = null)
        {
            return _repo.ExistsByFullNameAndPhone(fullName, phone, ignoreVisitorId);
        }

        public bool ExistsByFullNameAndPhoneForCreator(string fullName, string phone, string createdByUserId, int? ignoreVisitorId = null)
        {
            return _repo.ExistsByFullNameAndPhoneForCreator(fullName, phone, createdByUserId, ignoreVisitorId);
        }
    }
}
