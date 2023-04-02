#include "IFileSystemWalker.h"
#include "ImgProvider.h"

#include <gmock/gmock.h>
#include <gtest/gtest.h>

#ifdef __linux__
#include <filesystem>
#endif

using ::testing::Eq;
using ::testing::Exactly;
using ::testing::Return;

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
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(Exactly(1));
    imgProvider.requestPixmap("", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_ImgProvider_WHEN_image_with_underscore_next_is_requested_THEN_next_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getNextFile()).Times(Exactly(1));
    imgProvider.requestPixmap("test_next", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_ImgProvider_WHEN_image_with_underscore_prev_is_requested_THEN_prev_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getPrevFile()).Times(Exactly(1));
    imgProvider.requestPixmap("sometext42_prev", &size, size);
}

TEST_F(
    ImgProviderTest,
    GIVEN_ImgProvider_WHEN_image_with_underscore_and_some_other_suffix_is_requested_THEN_current_file_is_loaded)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(Exactly(1));
    imgProvider.requestPixmap("foo_bar", &size, size);
}

TEST_F(ImgProviderTest,
       GIVEN_some_request_to_ImgProvider_WHEN_empty_image_is_to_be_loaded_THEN_1x1_pixmap_is_returned)
{
    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile()).Times(Exactly(1)).WillOnce(Return(QString("")));
    auto pixmap = imgProvider.requestPixmap("", &size, size);
    EXPECT_THAT(size.width(), Eq(1));
    EXPECT_THAT(size.height(), Eq(1));
    EXPECT_THAT(pixmap.width(), Eq(1));
    EXPECT_THAT(pixmap.height(), Eq(1));
}

#ifdef __linux__
TEST_F(ImgProviderTest,
       GIVEN_request_to_current_file_WHEN_lenna_test_image_is_loaded_THEN_40x40_pixmap_is_returned)
{
    auto pathToBinary = std::filesystem::canonical("/proc/self/exe").parent_path();
    auto lennaImgFile = pathToBinary.append("lenna.jpg");
    if (not std::filesystem::exists(lennaImgFile))
    {
        std::cerr << "Lenna test file not found, skipping test "
                  << ::testing::UnitTest::GetInstance()->current_test_info()->name() << std::endl;
        return;
    }

    MockFileSystemWalker fileSystemWalker;
    ImgProvider imgProvider(&fileSystemWalker);
    EXPECT_CALL(fileSystemWalker, getCurrentFile())
        .Times(Exactly(1))
        .WillOnce(Return(QString(lennaImgFile.c_str())));
    auto pixmap = imgProvider.requestPixmap("", &size, size);
    EXPECT_THAT(size.width(), Eq(40));
    EXPECT_THAT(size.height(), Eq(40));
    EXPECT_THAT(pixmap.width(), Eq(40));
    EXPECT_THAT(pixmap.height(), Eq(40));
}
#endif // __linux__
