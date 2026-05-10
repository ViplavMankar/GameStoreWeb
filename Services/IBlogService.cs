using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IBlogService
{
    Task<List<BlogReadUserDto>> GetAllAsync();
    Task<BlogReadUserDto?> GetByIdWithUsernameAsync(Guid id);
    Task<BlogReadDto> CreateAsync(BlogCreateDto dto, Guid authorId);
    Task<BlogReadDto> EditAsync(Guid id, BlogEditDto dto, Guid authorId);
    Task<bool> DeleteAsync(Guid id, Guid authorId);
    Task<List<BlogReadUserDto>> GetMyBlogsAsync(Guid userId);
    Task<BlogReadDto?> GetMySingleBlogAsync(Guid userId, Guid blogId);
}
