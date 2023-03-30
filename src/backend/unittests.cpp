#include <gtest/gtest.h>

#include <QGuiApplication>

int main(int argc, char** argv)
{
    QGuiApplication a(argc, argv);
    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
