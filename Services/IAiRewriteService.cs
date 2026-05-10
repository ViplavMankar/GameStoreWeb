using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IAiRewriteService
{
    Task<RewriteResult> RewriteAsync(RewriteRequest req, CancellationToken ct);
}
