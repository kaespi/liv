#include "FileSystemWalker.h"
#include "IDir.h"
#include "IFileSystemClassesFactory.h"

#include <gmock/gmock.h>
#include <gtest/gtest.h>
#include <memory>

using ::testing::_;
using ::testing::Eq;
using ::testing::NiceMock;

class MockDir : public IDir
{
  public:
    MOCK_METHOD(void, setPath, (const QString&), (override));
    MOCK_METHOD(void, setCurrentPath, (), (override));
    MOCK_METHOD(QString, absolutePath, (), (const, override));
    MOCK_METHOD(QStringList, entryList, (const QStringList&, QDir::Filters, QDir::SortFlags),
                (const, override));
};

class FileSystemMockFactory : public IFileSystemClassesFactory
{
  public:
    FileSystemMockFactory() { m_ptrDir = new NiceMock<MockDir>; };
    virtual ~FileSystemMockFactory() = default;

    virtual std::unique_ptr<IDir> createDirClass() const override
    {
        return std::unique_ptr<MockDir>(m_ptrDir);
    }

    NiceMock<MockDir>* m_ptrDir{nullptr};
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
    EXPECT_THAT(fileSystemWalker.getCurrentFile(), Eq(QString{""}));
    EXPECT_THAT(fileSystemWalker.getNextFile(), Eq(QString{""}));
    EXPECT_THAT(fileSystemWalker.getPrevFile(), Eq(QString{""}));
}
