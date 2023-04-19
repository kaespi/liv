#include "FileSystemWalker.h"
#include "IDir.h"
#include "IFileSystem.h"
#include "IFileSystemClassesFactory.h"

#include <gmock/gmock.h>
#include <gtest/gtest.h>
#include <memory>

using ::testing::_; // NOLINT
using ::testing::Eq;
using ::testing::NiceMock;
using ::testing::Return;

class MockDir : public IDir
{
  public:
    MOCK_METHOD(void, setPath, (const QString&), (override));
    MOCK_METHOD(void, setCurrentPath, (), (override));
    MOCK_METHOD(QString, absolutePath, (), (const, override));
    MOCK_METHOD(QStringList, entryList, (const QStringList&, QDir::Filters, QDir::SortFlags),
                (const, override));
};

class MockFileSystem : public IFileSystem
{
  public:
    MOCK_METHOD(bool, fileExists, (const QString&), (const, override));
};

class FileSystemMockFactory : public IFileSystemClassesFactory
{
  public:
    FileSystemMockFactory() : m_ptrDir(new NiceMock<MockDir>), m_ptrFileSystem(new NiceMock<MockFileSystem>)
    {}

    [[nodiscard]] std::unique_ptr<IDir> createDirClass() const override
    {
        return std::unique_ptr<MockDir>(m_ptrDir);
    }

    [[nodiscard]] std::unique_ptr<IFileSystem> createFileSystemClass() const override
    {
        return std::unique_ptr<MockFileSystem>(m_ptrFileSystem);
    }

    NiceMock<MockDir>* m_ptrDir{nullptr};
    NiceMock<MockFileSystem>* m_ptrFileSystem{nullptr};
};

class FileSystemWalkerTest : public ::testing::Test
{
  protected:
};

TEST_F(
    FileSystemWalkerTest,
    GIVEN_FileSystemWalker_is_initialized_with_inexistent_file_WHEN_getting_file_THEN_an_empty_string_is_returned)
{
    FileSystemMockFactory fsFactoryMock;
    QList<QByteArray> fileTypes{};
    FileSystemWalker fileSystemWalker("inexistent_file.jpg", &fsFactoryMock, fileTypes);
    EXPECT_CALL(*fsFactoryMock.m_ptrFileSystem, fileExists(_)).WillRepeatedly(Return(false));
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq(QString{""}));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq(QString{""}));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq(QString{""}));
}

TEST_F(
    FileSystemWalkerTest,
    GIVEN_FileSystemWalker_is_initialized_with_existing_file_WHEN_getting_file_THEN_the_same_file_is_always_returned)
{
    const QString someFolder{"/xyz"};
    const QString existingFile{"a.jpg"};
    const QString fullPathExistingFile{someFolder + "/" + existingFile};

    FileSystemMockFactory fsFactoryMock;
    QList<QByteArray> fileTypes{};
    EXPECT_CALL(*fsFactoryMock.m_ptrDir, entryList(_, _, _))
        .WillRepeatedly(Return(QStringList{existingFile}));
    EXPECT_CALL(*fsFactoryMock.m_ptrDir, absolutePath()).WillRepeatedly(Return(someFolder));
    EXPECT_CALL(*fsFactoryMock.m_ptrFileSystem, fileExists(_)).WillRepeatedly(Return(true));

    FileSystemWalker fileSystemWalker(existingFile, &fsFactoryMock, fileTypes);
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq(fullPathExistingFile));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq(fullPathExistingFile));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq(fullPathExistingFile));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq(fullPathExistingFile));
}

TEST_F(FileSystemWalkerTest,
       GIVEN_browsing_folder_with_multiple_files_WHEN_getting_next_file_THEN_the_files_are_returned_in_order)
{
    FileSystemMockFactory fsFactoryMock;
    QList<QByteArray> fileTypes{};
    EXPECT_CALL(*fsFactoryMock.m_ptrDir, entryList(_, _, _))
        .WillRepeatedly(Return(QStringList{"a.jpg", "b.jpg", "c.jpg"}));
    EXPECT_CALL(*fsFactoryMock.m_ptrFileSystem, fileExists(_)).WillRepeatedly(Return(true));

    FileSystemWalker fileSystemWalker("a.jpg", &fsFactoryMock, fileTypes);
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq("/a.jpg"));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq("/b.jpg"));
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq("/b.jpg"));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq("/c.jpg"));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq("/a.jpg"));
}

TEST_F(
    FileSystemWalkerTest,
    GIVEN_browsing_folder_with_multiple_files_WHEN_getting_previous_file_THEN_the_files_are_returned_in_order)
{
    FileSystemMockFactory fsFactoryMock;
    QList<QByteArray> fileTypes{};
    EXPECT_CALL(*fsFactoryMock.m_ptrDir, entryList(_, _, _))
        .WillRepeatedly(Return(QStringList{"a.jpg", "b.jpg", "c.jpg"}));
    EXPECT_CALL(*fsFactoryMock.m_ptrFileSystem, fileExists(_)).WillRepeatedly(Return(true));

    FileSystemWalker fileSystemWalker("a.jpg", &fsFactoryMock, fileTypes);
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq("/a.jpg"));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq("/c.jpg"));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq("/b.jpg"));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq("/a.jpg"));
}
