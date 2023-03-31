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

TEST_F(ImgProviderTest,
       GIVEN_ImgProvider_WHEN_image_without_underscore_is_requested_THEN_current_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(::testing::Exactly(1));
    imgProvider.requestPixmap("", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_ImgProvider_WHEN_image_with_underscore_next_is_requested_THEN_next_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getNextFile()).Times(::testing::Exactly(1));
    imgProvider.requestPixmap("test_next", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_ImgProvider_WHEN_image_with_underscore_prev_is_requested_THEN_prev_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getPrevFile()).Times(::testing::Exactly(1));
    imgProvider.requestPixmap("sometext42_prev", &size, size);
}
