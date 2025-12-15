using System.Collections.Concurrent;

namespace IT_13FinalProject.Services
{
    public record ClaimDocument(string FileName, string ContentType, byte[] Content);

    public interface IClaimDocumentStore
    {
        void Save(int billId, string documentKey, ClaimDocument document);
        bool TryGet(int billId, string documentKey, out ClaimDocument? document);
        IReadOnlyDictionary<string, ClaimDocument> GetAllForBill(int billId);
    }

    public class InMemoryClaimDocumentStore : IClaimDocumentStore
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ClaimDocument>> _store = new();

        public void Save(int billId, string documentKey, ClaimDocument document)
        {
            var billDocs = _store.GetOrAdd(billId, _ => new ConcurrentDictionary<string, ClaimDocument>(StringComparer.OrdinalIgnoreCase));
            billDocs[documentKey] = document;
        }

        public bool TryGet(int billId, string documentKey, out ClaimDocument? document)
        {
            document = null;

            if (!_store.TryGetValue(billId, out var billDocs))
                return false;

            if (!billDocs.TryGetValue(documentKey, out var doc))
                return false;

            document = doc;
            return true;
        }

        public IReadOnlyDictionary<string, ClaimDocument> GetAllForBill(int billId)
        {
            if (_store.TryGetValue(billId, out var billDocs))
                return billDocs;

            return new Dictionary<string, ClaimDocument>();
        }
    }
}
