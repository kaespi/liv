cmake_minimum_required(VERSION 3.16)

project(liv_backend VERSION 0.0.1 LANGUAGES CXX)

add_library(liv_backend_lib STATIC)

target_sources(liv_backend_lib
    PRIVATE
        ImgProvider.cpp
        ImgProvider.h
        BackendManager.cpp
        BackendManager.h
        FileSystemWalker.cpp
        FileSystemWalker.h
)

target_include_directories(liv_backend_lib
    PUBLIC
        "${CMAKE_CURRENT_SOURCE_DIR}"
)

target_link_libraries(liv_backend_lib
    PRIVATE Qt6::Quick)

# **** unittests ****
add_executable(${PROJECT_NAME}-test)

target_sources(${PROJECT_NAME}-test
    PRIVATE
        unittests.cpp
)

target_link_libraries(${PROJECT_NAME}-test
    PRIVATE
        liv_backend_lib
        GTest::gtest_main
)

add_test(${PROJECT_NAME}-gtests ${PROJECT_NAME}-test)
