namespace OPCBS.Web.DTOs;

public record BlogDto(Guid Id, string Title, string Summary, string Content, DateTime CreatedAt);
