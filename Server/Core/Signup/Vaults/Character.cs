using Altruist;
using Altruist.Persistence;
using Altruist.UORM;

using Microsoft.Extensions.DependencyInjection;

namespace Server.Persistence;

[Vault("character")]
public class Character : VaultModel, IOnVaultCreate<Character>
{
    [VaultColumn("accountId")]
    public string AccountId { get; set; }

    [VaultColumn("Name")]
    public string Name { get; set; }

    public async Task<List<Character>> OnCreateAsync(IServiceProvider serviceProvider)
    {
        IVault<Account> vaultRepo = serviceProvider.GetRequiredService<IVault<Account>>();
        Account account = (await vaultRepo.Where(a => a.Username == "admin").FirstOrDefaultAsync())!;
        var character = new Character() { AccountId = account!.StorageId, Name = "TestCharacter" };
        return await Task.FromResult(new List<Character> { character });
    }
}
