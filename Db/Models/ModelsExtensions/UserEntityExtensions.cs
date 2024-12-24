using Microsoft.AspNetCore.Identity;

namespace Db.Models.ModelsExtensions;

public static class UserEntityExtensions
{
    public static async Task<UserEntity> UpdateUserEntiyAsync(this UserEntity userEntity,
        UserManager<UserEntity> userManager)
    {
        var updateAsync = await userManager.UpdateAsync(userEntity);
        if (!updateAsync.Succeeded)
            throw new Exception(updateAsync.Errors.ToString());

        return userEntity;
    }
}