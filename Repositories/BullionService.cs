using Microsoft.Data.SqlClient;

namespace SL_Bullion.Repositories
{
    public class BullionService
    {
        private readonly SqlService _sqlService;

        public BullionService(SqlService sqlService)
        {
            _sqlService = sqlService;
        }

        public async Task<object> getCategoryDetails(string user)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@user", user)
            };

            var result = await _sqlService.executeReaderList("category_Get", parameters);
            return result;
        }

        public async Task<object> getSubCategoryDetails(string user)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@user", user)
            };
            var result = await _sqlService.executeReaderList("subCategory_Get", parameters);
            return result;
        }

        public async Task<object> getJewelleryByUser(string user)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@user", user)
            };
            var result = await _sqlService.executeReaderList("JewelleryByUser_Get", parameters);
            return result;
        }

        public async Task<object> getJewelleryDetails(string user, int categoryId, int subCategoryId)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@user", user),
                new SqlParameter("@CategoryId", categoryId),
                new SqlParameter("@SubCategoryId", subCategoryId)
            };
            var result = await _sqlService.executeReaderList("jewellery_Get", parameters);
            return result;
        }

        public async Task<object> getSliderDetails(string user)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@user", user)
            };
            var result = await _sqlService.executeReaderList("SliderByUser_Get", parameters);
            return result;
        }
    }
}
