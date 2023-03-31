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

TEST_F(
    ImgProviderTest,
    GIVEN_ImgProvider_WHEN_image_with_underscore_and_some_other_suffix_is_requested_THEN_current_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(::testing::Exactly(1));
    imgProvider.requestPixmap("foo_bar", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_some_request_to_ImgProvider_WHEN_empty_image_is_to_be_loaded_THEN_1x1_pixmap_is_returned)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile())
        .Times(testing::Exactly(1))
        .WillOnce(testing::Return(QString("")));
    auto pixmap = imgProvider.requestPixmap("", &size, size);
    EXPECT_THAT(size.width(), testing::Eq(1));
    EXPECT_THAT(size.height(), testing::Eq(1));
    EXPECT_THAT(pixmap.width(), testing::Eq(1));
    EXPECT_THAT(pixmap.height(), testing::Eq(1));
}
