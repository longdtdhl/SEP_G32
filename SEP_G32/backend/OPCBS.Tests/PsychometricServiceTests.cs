using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCBS.Application.DTOs.Psychometric;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using Xunit;

namespace OPCBS.Tests;

public class PsychometricServiceTests
{
    [Fact]
    public async Task SubmitTestAsync_WithPHQ9_CalculatesScoreAndInterpretationCorrectly()
    {
        // Arrange
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var testId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var test = new PsychometricTest
        {
            Id = testId,
            Title = "PHQ-9",
            TestType = "PHQ9"
        };

        var questions = new List<PsychometricQuestion>();
        var answersDto = new List<AnswerDto>();

        for (int i = 0; i < 9; i++)
        {
            var qId = Guid.NewGuid();
            questions.Add(new PsychometricQuestion
            {
                Id = qId,
                TestId = testId,
                QuestionText = $"Question {i + 1}",
                QuestionNumber = i + 1,
                Category = "Depression",
                Test = test
            });

            answersDto.Add(new AnswerDto { QuestionId = qId, Score = 2 });
        }

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        var service = new PsychometricService(
            testRepo.Object,
            questionRepo.Object,
            submissionRepo.Object,
            answerRepo.Object,
            patientRepo.Object,
            apptRepo.Object,
            userRepo.Object,
            uow.Object
        );

        var submitDto = new SubmitTestDto
        {
            TestId = testId,
            Answers = answersDto
        };

        // Act
        var response = await service.SubmitTestAsync(submitDto, patientUserId, default);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(18, response.Data!.TotalScore);
        Assert.Equal("Trầm cảm trung bình nặng", response.Data.Interpretation);
    }

    [Fact]
    public async Task SubmitTestAsync_WithDASS21_CalculatesScoreAndInterpretationCorrectly()
    {
        // Arrange
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var testId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var test = new PsychometricTest
        {
            Id = testId,
            Title = "DASS-21",
            TestType = "DASS21"
        };

        var questions = new List<PsychometricQuestion>();
        var answersDto = new List<AnswerDto>();

        var categories = new[] { "Depression", "Anxiety", "Stress" };
        int questionNum = 1;
        foreach (var cat in categories)
        {
            for (int i = 0; i < 7; i++)
            {
                var qId = Guid.NewGuid();
                questions.Add(new PsychometricQuestion
                {
                    Id = qId,
                    TestId = testId,
                    QuestionText = $"Question {questionNum}",
                    QuestionNumber = questionNum++,
                    Category = cat,
                    Test = test
                });

                int score = cat == "Stress" ? 2 : 1;
                answersDto.Add(new AnswerDto { QuestionId = qId, Score = score });
            }
        }

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile> { new PatientProfile { Id = patientId, UserId = patientUserId, User = new User { Id = patientUserId, Email = "p@test.com", FullName = "Patient", PhoneNumber = "123", PasswordHash = "hash", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } } });
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        var service = new PsychometricService(
            testRepo.Object,
            questionRepo.Object,
            submissionRepo.Object,
            answerRepo.Object,
            patientRepo.Object,
            apptRepo.Object,
            userRepo.Object,
            uow.Object
        );

        var submitDto = new SubmitTestDto
        {
            TestId = testId,
            Answers = answersDto
        };

        // Act
        var response = await service.SubmitTestAsync(submitDto, patientUserId, default);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(14 + 14 + 28, response.Data!.TotalScore);
        Assert.Equal("Trầm cảm: Vừa, Lo âu: Vừa, Căng thẳng: Nặng", response.Data.Interpretation);
    }
}
