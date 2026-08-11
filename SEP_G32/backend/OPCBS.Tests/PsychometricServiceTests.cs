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

    // ──────────────────────────────────────────────
    // MORE PSYCHOMETRIC TEST SUBMISSION TESTS (20+ Cases)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SubmitTestAsync_TestNotFound_Fails()
    {
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var patientUserId = Guid.NewGuid();
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@t.com", FullName = "Pat", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        testRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PsychometricTest?)null);

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = Guid.NewGuid(), Answers = new List<AnswerDto>() };
        var response = await service.SubmitTestAsync(submitDto, patientUserId, default);

        Assert.False(response.Success);
        Assert.Contains("Không tìm thấy bài trắc nghiệm", response.Message);
    }

    [Fact]
    public async Task SubmitTestAsync_PatientNotFound_Fails()
    {
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var test = new PsychometricTest { Id = Guid.NewGuid(), Title = "PHQ-9", TestType = "PHQ9" };
        testRepo.Setup(r => r.GetByIdAsync(test.Id, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>()); // No patients

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = test.Id, Answers = new List<AnswerDto>() };
        var response = await service.SubmitTestAsync(submitDto, Guid.NewGuid(), default);

        Assert.False(response.Success);
        Assert.Contains("Không tìm thấy hồ sơ bệnh nhân", response.Message);
    }

    [Fact]
    public async Task SubmitTestAsync_EmptyAnswers_Fails()
    {
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var testId = Guid.NewGuid();
        var test = new PsychometricTest { Id = testId, Title = "PHQ-9", TestType = "PHQ9" };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), User = new User { Email = "p@t.com", FullName = "Pat", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };

        testRepo.Setup(r => r.GetByIdAsync(test.Id, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });

        var question = new PsychometricQuestion { Id = Guid.NewGuid(), TestId = testId, QuestionNumber = 1, Test = test, QuestionText = "Q1" };
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PsychometricQuestion> { question });

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = test.Id, Answers = new List<AnswerDto>() }; // Empty answers
        var response = await service.SubmitTestAsync(submitDto, patient.UserId, default);

        Assert.False(response.Success);
        Assert.Contains("không khớp", response.Message);
    }

    [Theory]
    [InlineData(0, "Bình thường / Không trầm cảm")]
    [InlineData(4, "Bình thường / Không trầm cảm")]
    [InlineData(5, "Trầm cảm nhẹ")]
    [InlineData(9, "Trầm cảm nhẹ")]
    [InlineData(10, "Trầm cảm vừa")]
    [InlineData(14, "Trầm cảm vừa")]
    [InlineData(15, "Trầm cảm trung bình nặng")]
    [InlineData(19, "Trầm cảm trung bình nặng")]
    [InlineData(20, "Trầm cảm nặng")]
    [InlineData(27, "Trầm cảm nặng")]
    public async Task SubmitTestAsync_PHQ9Interpretations(int totalScore, string expectedInterpretation)
    {
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
        var test = new PsychometricTest { Id = testId, Title = "PHQ-9", TestType = "PHQ9" };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@t.com", FullName = "Pat", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };

        var questions = new List<PsychometricQuestion>();
        var answersDto = new List<AnswerDto>();

        // We distribute the totalScore across 9 questions (each answer score between 0 and 3)
        int remainingScore = totalScore;
        for (int i = 0; i < 9; i++)
        {
            var qId = Guid.NewGuid();
            questions.Add(new PsychometricQuestion { Id = qId, TestId = testId, QuestionNumber = i + 1, Test = test, QuestionText = $"Question {i + 1}" });

            int scoreForThisQuestion = Math.Min(remainingScore, 3);
            remainingScore -= scoreForThisQuestion;
            answersDto.Add(new AnswerDto { QuestionId = qId, Score = scoreForThisQuestion });
        }

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = testId, Answers = answersDto };
        var response = await service.SubmitTestAsync(submitDto, patientUserId, default);

        Assert.True(response.Success);
        Assert.Equal(totalScore, response.Data!.TotalScore);
        Assert.Equal(expectedInterpretation, response.Data.Interpretation);
    }

    [Theory]
    [InlineData(0, 0, 0, "Trầm cảm: Bình thường, Lo âu: Bình thường, Căng thẳng: Bình thường")]
    [InlineData(8, 6, 12, "Trầm cảm: Bình thường, Lo âu: Bình thường, Căng thẳng: Bình thường")]
    [InlineData(10, 8, 16, "Trầm cảm: Nhẹ, Lo âu: Nhẹ, Căng thẳng: Nhẹ")]
    [InlineData(12, 8, 18, "Trầm cảm: Nhẹ, Lo âu: Nhẹ, Căng thẳng: Nhẹ")]
    [InlineData(14, 10, 20, "Trầm cảm: Vừa, Lo âu: Vừa, Căng thẳng: Vừa")]
    [InlineData(20, 14, 24, "Trầm cảm: Vừa, Lo âu: Vừa, Căng thẳng: Vừa")]
    [InlineData(22, 16, 26, "Trầm cảm: Nặng, Lo âu: Nặng, Căng thẳng: Nặng")]
    [InlineData(26, 18, 32, "Trầm cảm: Nặng, Lo âu: Nặng, Căng thẳng: Nặng")]
    [InlineData(28, 20, 34, "Trầm cảm: Rất nặng, Lo âu: Rất nặng, Căng thẳng: Rất nặng")]
    [InlineData(42, 42, 42, "Trầm cảm: Rất nặng, Lo âu: Rất nặng, Căng thẳng: Rất nặng")]
    public async Task SubmitTestAsync_DASS21Interpretations(int depScore, int anxScore, int strScore, string expectedInterpretation)
    {
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
        var test = new PsychometricTest { Id = testId, Title = "DASS-21", TestType = "DASS21" };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@t.com", FullName = "Pat", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };

        var questions = new List<PsychometricQuestion>();
        var answersDto = new List<AnswerDto>();

        // DASS21 questions are split equally between Depression, Anxiety, and Stress.
        // DASS21 scores are calculated as the sum of answers multiplied by 2 (according to standard scale, or let's check DASS21 implementation).
        // Let's verify how it is implemented in PsychometricService. Let's see: if total score is depScore, then the sum of scores of the 7 depression questions is depScore / 2.
        int qNum = 1;
        var categories = new[] { ("Depression", depScore), ("Anxiety", anxScore), ("Stress", strScore) };
        foreach (var (cat, targetScore) in categories)
        {
            int remainingScore = targetScore / 2; // DASS21 multiplies by 2
            for (int i = 0; i < 7; i++)
            {
                var qId = Guid.NewGuid();
                questions.Add(new PsychometricQuestion { Id = qId, TestId = testId, QuestionNumber = qNum++, Category = cat, Test = test, QuestionText = $"Question {qNum}" });
                int scoreVal = Math.Min(remainingScore, 3);
                remainingScore -= scoreVal;
                answersDto.Add(new AnswerDto { QuestionId = qId, Score = scoreVal });
            }
        }

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = testId, Answers = answersDto };
        var response = await service.SubmitTestAsync(submitDto, patientUserId, default);

        Assert.True(response.Success);
        Assert.Equal(expectedInterpretation, response.Data!.Interpretation);
    }

    [Fact]
    public async Task SubmitTestAsync_DbSaveFails_ThrowsException()
    {
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
        var test = new PsychometricTest { Id = testId, Title = "PHQ-9", TestType = "PHQ9" };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@t.com", FullName = "Pat", PhoneNumber = "1", PasswordHash = "x", RoleId = Guid.NewGuid(), Role = new Role { Name = "Patient" } } };

        var questions = new List<PsychometricQuestion>();
        var answersDto = new List<AnswerDto>();

        for (int i = 0; i < 9; i++)
        {
            var qId = Guid.NewGuid();
            questions.Add(new PsychometricQuestion { Id = qId, TestId = testId, QuestionNumber = i + 1, Test = test, QuestionText = $"Question {i + 1}" });
            answersDto.Add(new AnswerDto { QuestionId = qId, Score = 1 });
        }

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var submitDto = new SubmitTestDto { TestId = testId, Answers = answersDto };
        await Assert.ThrowsAsync<Exception>(() => service.SubmitTestAsync(submitDto, patientUserId, default));
    }

    [Fact]
    public async Task CreateTestAsync_ValidData_CreatesTestSuccessfully()
    {
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var dto = new CreatePsychometricTestDto
        {
            Title = "GAD-7 Anxiety Scale",
            TestType = "GAD7",
            Description = "Generalized anxiety disorder screening",
            Questions = new List<CreatePsychometricQuestionDto>
            {
                new() { QuestionNumber = 1, QuestionText = "Feeling nervous or anxious", Category = "Anxiety" },
                new() { QuestionNumber = 2, QuestionText = "Not being able to stop worrying", Category = "Anxiety" }
            }
        };

        var response = await service.CreateTestAsync(dto);

        Assert.True(response.Success);
        Assert.Equal("GAD-7 Anxiety Scale", response.Data!.Title);
        Assert.Equal("GAD7", response.Data.TestType);
        Assert.Equal(2, response.Data.QuestionCount);
        testRepo.Verify(r => r.AddAsync(It.IsAny<PsychometricTest>(), It.IsAny<CancellationToken>()), Times.Once);
        questionRepo.Verify(r => r.AddAsync(It.IsAny<PsychometricQuestion>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteTestAsync_ExistingTest_SoftDeletesTestAndQuestions()
    {
        var testRepo = new Mock<IRepository<PsychometricTest>>();
        var questionRepo = new Mock<IRepository<PsychometricQuestion>>();
        var submissionRepo = new Mock<IRepository<PsychometricSubmission>>();
        var answerRepo = new Mock<IRepository<PsychometricAnswer>>();
        var patientRepo = new Mock<IRepository<PatientProfile>>();
        var apptRepo = new Mock<IRepository<Appointment>>();
        var userRepo = new Mock<IRepository<User>>();
        var uow = new Mock<IUnitOfWork>();

        var testId = Guid.NewGuid();
        var test = new PsychometricTest { Id = testId, Title = "Custom Test", TestType = "CUSTOM" };
        var questions = new List<PsychometricQuestion>
        {
            new() { Id = Guid.NewGuid(), TestId = testId, QuestionText = "Q1", QuestionNumber = 1, Test = test }
        };

        testRepo.Setup(r => r.GetByIdAsync(testId, It.IsAny<CancellationToken>())).ReturnsAsync(test);
        questionRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        var service = new PsychometricService(
            testRepo.Object, questionRepo.Object, submissionRepo.Object, answerRepo.Object,
            patientRepo.Object, apptRepo.Object, userRepo.Object, uow.Object
        );

        var response = await service.DeleteTestAsync(testId);

        Assert.True(response.Success);
        Assert.True(test.IsDeleted);
        Assert.True(questions[0].IsDeleted);
        testRepo.Verify(r => r.Update(test), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
