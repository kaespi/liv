#include "IFileSystemWalker.h"
#include "ImgProvider.h"

#include <gmock/gmock.h>
#include <gtest/gtest.h>

class MockFileSystemWalker : public IFileSystemWalker
{
  public:
    MOCK_METHOD(QString, getCurrentFile, (), (const, override));
    MOCK_METHOD(QString, getNextFile, (), (override));
    MOCK_METHOD(QString, getPrevFile, (), (override));
};

class ImgProviderTest : public ::testing::Test
{
  protected:
    QSize size;
};

TEST_F(ImgProviderTest, WHEN_image_without_underscore_is_requested_THEN_current_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(::testing::Exactly(1));
    imgProvider.requestPixmap("", &size, size);
}
